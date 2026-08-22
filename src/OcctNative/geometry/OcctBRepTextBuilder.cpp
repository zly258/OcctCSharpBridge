#include "geometry/OcctBRepTextBuilder.hxx"

#include <BRepPrimAPI_MakePrism.hxx>
#include <Font_FontAspect.hxx>
#include <Font_StrictLevel.hxx>
#include <NCollection_String.hxx>
#include <Precision.hxx>
#include <StdPrs_BRepFont.hxx>
#include <StdPrs_BRepTextBuilder.hxx>
#include <TCollection_AsciiString.hxx>
#include <gp_Ax3.hxx>
#include <gp_Dir.hxx>
#include <gp_Pnt.hxx>
#include <gp_Vec.hxx>

#include <algorithm>
#include <cmath>
#include <stdexcept>
#include <string>
#include <vector>

namespace
{
    Font_FontAspect fontAspect(bool bold, bool italic)
    {
        if (bold && italic) return Font_FA_BoldItalic;
        if (bold) return Font_FA_Bold;
        if (italic) return Font_FA_Italic;
        return Font_FA_Regular;
    }

    void requirePositive(double value, const char* name)
    {
        if (!std::isfinite(value) || value <= 0.0)
            throw std::invalid_argument(std::string(name) + " must be finite and greater than zero.");
    }

    gp_Pnt point(OcctPoint3d value)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument("Point coordinates must be finite.");
        return gp_Pnt(value.x, value.y, value.z);
    }

    gp_Dir direction(OcctVector3d value)
    {
        if (!std::isfinite(value.x) || !std::isfinite(value.y) || !std::isfinite(value.z))
            throw std::invalid_argument("Direction vector must be finite.");
        const gp_Vec vector(value.x, value.y, value.z);
        if (vector.SquareMagnitude() <= Precision::SquareConfusion())
            throw std::invalid_argument("Direction vector must not be zero.");
        return gp_Dir(vector);
    }

    Standard_Utf32Char fallbackPunctuation(Standard_Utf32Char ch)
    {
        switch (ch)
        {
        case 0xFF08: return '(';  // Full-width （ -> (
        case 0xFF09: return ')';  // Full-width ） -> )
        case 0xFF1A: return ':';  // Full-width ： -> :
        case 0xFF0C: return ',';  // Full-width ， -> ,
        case 0xFF1B: return ';';  // Full-width ； -> ;
        case 0xFF01: return '!';  // Full-width ！ -> !
        case 0xFF1F: return '?';  // Full-width ？ -> ?
        case 0x3010: return '[';  // 【 -> [
        case 0x3011: return ']';  // 】 -> ]
        case 0xFF3B: return '[';  // ［ -> [
        case 0xFF3D: return ']';  // ］ -> ]
        case 0x3008: return '<';  // 〈 -> <
        case 0x3009: return '>';  // 〉 -> >
        case 0x300A: return '<';  // 《 -> <
        case 0x300B: return '>';  // 》 -> >
        case 0x201C: case 0x201D: return '"';  // “ ” -> "
        case 0x2018: case 0x2019: return '\''; // ‘ ’ -> '
        case 0x3001: return ',';  // 、 -> ,
        case 0x3002: return '.';  // 。 -> .
        case 0x2014: return '-';  // — -> -
        case 0x2026: return '.';  // … -> .
        case 0x00B7: return '.';  // · -> .
        case 0xFFE5: return '$';  // ￥ -> $
        default: return 0;
        }
    }

    bool initializeFont(
        StdPrs_BRepFont& font,
        const char* fontName,
        Font_FontAspect aspect,
        double height)
    {
        const std::string requested = fontName == nullptr ? std::string() : std::string(fontName);
        std::vector<std::string> candidates;
        if (!requested.empty()) candidates.push_back(requested);
#if defined(_WIN32)
        candidates.emplace_back("Microsoft YaHei");
        candidates.emplace_back("Microsoft YaHei UI");
        candidates.emplace_back("SimSun");
        candidates.emplace_back("SimHei");
        candidates.emplace_back("KaiTi");
        candidates.emplace_back("FangSong");
        candidates.emplace_back("C:\\Windows\\Fonts\\msyh.ttc");
        candidates.emplace_back("C:\\Windows\\Fonts\\simsun.ttc");
        candidates.emplace_back("C:\\Windows\\Fonts\\simhei.ttf");
        candidates.emplace_back("Segoe UI");
        candidates.emplace_back("Arial");
#elif defined(__linux__)
        candidates.emplace_back("Noto Sans CJK SC");
        candidates.emplace_back("WenQuanYi Micro Hei");
        candidates.emplace_back("AR PL UMing CN");
        candidates.emplace_back("Noto Sans");
        candidates.emplace_back("DejaVu Sans");
#else
        candidates.emplace_back("DejaVu Sans");
#endif

        std::vector<std::string> attempted;
        for (const std::string& candidate : candidates)
        {
            if (candidate.empty()
                || std::find(attempted.begin(), attempted.end(), candidate) != attempted.end())
            {
                continue;
            }

            attempted.push_back(candidate);

            if (candidate.find('/') != std::string::npos || candidate.find('\\') != std::string::npos)
            {
                if (font.Init(TCollection_AsciiString(candidate.c_str()), aspect, height))
                {
                    return true;
                }
            }

            if (font.FindAndInit(
                    TCollection_AsciiString(candidate.c_str()),
                    aspect,
                    height,
                    Font_StrictLevel_Any))
            {
                return true;
            }
        }
        return false;
    }
}

namespace OcctModelingInternal
{
    TopoDS_Shape buildBRepText(
        const char* utf8Text,
        const char* fontName,
        OcctPoint3d position,
        OcctVector3d normal,
        OcctVector3d xDirection,
        double height,
        double extrusionDepth,
        bool bold,
        bool italic,
        Graphic3d_HorizontalTextAlignment horizontalAlignment,
        Graphic3d_VerticalTextAlignment verticalAlignment)
    {
        if (utf8Text == nullptr || utf8Text[0] == '\0')
            throw std::invalid_argument("Text is empty.");
        requirePositive(height, "Text height");
        if (!std::isfinite(extrusionDepth) || extrusionDepth < 0.0)
            throw std::invalid_argument("Text extrusion depth must be non-negative.");

        StdPrs_BRepFont font;
        if (!initializeFont(font, fontName, fontAspect(bold, italic), height))
            throw std::runtime_error("No usable system font was found for BRep text generation.");

        const gp_Dir normalDirection = direction(normal);
        const gp_Dir xAxisDirection = direction(xDirection);
        if (std::abs(normalDirection.Dot(xAxisDirection)) > 1.0 - Precision::Angular())
            throw std::invalid_argument("Text x-direction must not be parallel to the text normal.");

        // Normalize text: if the current font is missing CJK punctuation such as full-width
        // parentheses '（' (U+FF08) / '）' (U+FF09), fallback to standard punctuation so it never disappears.
        NCollection_Utf32String normalizedText;
        NCollection_UtfIterator<Standard_Utf8Char> iter(reinterpret_cast<const Standard_Utf8Char*>(utf8Text));
        while (*iter != 0)
        {
            const Standard_Utf32Char ch = *iter;
            if (!font.HasGlyph(ch))
            {
                const Standard_Utf32Char fallback = fallbackPunctuation(ch);
                normalizedText += (fallback != 0 && font.HasGlyph(fallback)) ? fallback : ch;
            }
            else
            {
                normalizedText += ch;
            }
            ++iter;
        }

        StdPrs_BRepTextBuilder builder;
        const gp_Ax3 placement(point(position), normalDirection, xAxisDirection);
        TopoDS_Shape result = builder.Perform(
            font,
            normalizedText,
            placement,
            horizontalAlignment,
            verticalAlignment);
        if (result.IsNull())
            throw std::runtime_error("BRep text generation returned an empty shape.");

        if (extrusionDepth > Precision::Confusion())
        {
            BRepPrimAPI_MakePrism prism(
                result,
                gp_Vec(normalDirection) * extrusionDepth,
                Standard_True,
                Standard_True);
            if (!prism.IsDone() || prism.Shape().IsNull())
                throw std::runtime_error("BRep text extrusion failed.");
            result = prism.Shape();
        }
        return result;
    }
}
