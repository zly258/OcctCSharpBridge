#pragma once

#include <algorithm>
#include <cctype>
#include <filesystem>
#include <string>

namespace OcctBridge
{
    inline std::filesystem::path pathFromUtf8(const char* utf8Path)
    {
        if (utf8Path == nullptr || *utf8Path == '\0') return {};
#if defined(_WIN32)
        return std::filesystem::u8path(utf8Path);
#else
        return std::filesystem::path(utf8Path);
#endif
    }

    inline std::string pathToUtf8(const std::filesystem::path& path)
    {
        return path.u8string();
    }

    inline std::string lowerExtension(const std::filesystem::path& path)
    {
        std::string value = path.extension().u8string();
        std::transform(value.begin(), value.end(), value.begin(), [](unsigned char ch)
        {
            return static_cast<char>(std::tolower(ch));
        });
        return value;
    }
}
