cmake_minimum_required(VERSION 3.21)

project(OcctNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS OFF)

get_filename_component(BRIDGE_REPOSITORY_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/../.." ABSOLUTE)
set(BRIDGE_CONTRACT_FILE "${BRIDGE_REPOSITORY_ROOT}/bridge-contract.json")
if(NOT EXISTS "${BRIDGE_CONTRACT_FILE}")
    message(FATAL_ERROR "Bridge contract was not found: ${BRIDGE_CONTRACT_FILE}")
endif()
file(READ "${BRIDGE_CONTRACT_FILE}" BRIDGE_CONTRACT_JSON)
string(JSON BRIDGE_VERSION GET "${BRIDGE_CONTRACT_JSON}" bridgeVersion)
string(JSON BRIDGE_ABI_VERSION GET "${BRIDGE_CONTRACT_JSON}" nativeAbiVersion)
string(JSON REQUIRED_OCCT_VERSION GET "${BRIDGE_CONTRACT_JSON}" occtVersion)

if(NOT BRIDGE_VERSION OR NOT BRIDGE_ABI_VERSION OR NOT REQUIRED_OCCT_VERSION)
    message(FATAL_ERROR "bridge-contract.json is missing native build metadata.")
endif()
string(REPLACE "." ";" REQUIRED_OCCT_VERSION_PARTS "${REQUIRED_OCCT_VERSION}")
list(LENGTH REQUIRED_OCCT_VERSION_PARTS REQUIRED_OCCT_VERSION_PART_COUNT)
if(NOT REQUIRED_OCCT_VERSION_PART_COUNT EQUAL 3)
    message(FATAL_ERROR "The contracted OCCT version must use major.minor.maintenance format: ${REQUIRED_OCCT_VERSION}")
endif()
list(GET REQUIRED_OCCT_VERSION_PARTS 0 REQUIRED_OCCT_MAJOR)
list(GET REQUIRED_OCCT_VERSION_PARTS 1 REQUIRED_OCCT_MINOR)
list(GET REQUIRED_OCCT_VERSION_PARTS 2 REQUIRED_OCCT_MAINTENANCE)

if(WIN32)
    set(OCCT_DEFAULT_ROOT "D:/tools/occt-vc144-64" CACHE PATH "Default OCCT installation root used when OCCT_ROOT is not supplied")
    set(OCCT_ROOT "$ENV{OCCT_ROOT}" CACHE PATH "OCCT installation root")
    if(NOT OCCT_ROOT AND OCCT_DEFAULT_ROOT)
        set(OCCT_ROOT "${OCCT_DEFAULT_ROOT}" CACHE PATH "OCCT installation root" FORCE)
    endif()
    if(NOT OCCT_ROOT OR NOT EXISTS "${OCCT_ROOT}")
        message(FATAL_ERROR "OCCT SDK root was not found: ${OCCT_ROOT}. Set OCCT_ROOT, pass -DOCCT_ROOT=<path>, or install OCCT at ${OCCT_DEFAULT_ROOT}.")
    endif()

    set(OCCT_INCLUDE_DIR "${OCCT_ROOT}/inc" CACHE PATH "OCCT include directory")
    set(OCCT_LIB_DIR "${OCCT_ROOT}/win64/vc14/lib" CACHE PATH "OCCT library directory")
    set(OCCT_BIN_DIR "${OCCT_ROOT}/win64/vc14/bin" CACHE PATH "OCCT runtime directory")
    set(OCCT_CASROOT "${OCCT_ROOT}")
elseif(UNIX AND NOT APPLE)
    set(OCCT_DEFAULT_ROOT "/usr/local" CACHE PATH "Default Linux OCCT installation root used when OCCT_ROOT is not supplied")
    set(OCCT_ROOT "$ENV{OCCT_ROOT}" CACHE PATH "OCCT installation root")
    if(NOT OCCT_ROOT)
        set(OCCT_ROOT "${OCCT_DEFAULT_ROOT}" CACHE PATH "OCCT installation root" FORCE)
    endif()

    # Linux branch defaults requested for a conventional /usr/local OCCT install.
    set(OCCT_INCLUDE_DIR "/usr/local/include/opencascade" CACHE PATH "OCCT include directory")
    set(OCCT_LIB_DIR "/usr/local/lib" CACHE PATH "OCCT library directory")
    if(NOT OCCT_ROOT STREQUAL "/usr/local")
        set(OCCT_INCLUDE_DIR "${OCCT_ROOT}/include/opencascade" CACHE PATH "OCCT include directory" FORCE)
        set(OCCT_LIB_DIR "${OCCT_ROOT}/lib" CACHE PATH "OCCT library directory" FORCE)
    endif()
    set(OCCT_CASROOT "${OCCT_ROOT}")
else()
    message(FATAL_ERROR "OcctCSharpBridge currently supports Windows x64 and Linux x64 builds only.")
endif()

if(NOT EXISTS "${OCCT_INCLUDE_DIR}/Standard.hxx")
    message(FATAL_ERROR "Required OCCT header was not found: ${OCCT_INCLUDE_DIR}/Standard.hxx")
endif()
if(NOT EXISTS "${OCCT_INCLUDE_DIR}/Standard_Version.hxx")
    message(FATAL_ERROR "Required OCCT version header was not found: ${OCCT_INCLUDE_DIR}/Standard_Version.hxx")
endif()

file(READ "${OCCT_INCLUDE_DIR}/Standard_Version.hxx" _occt_version_header)
if(NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MAJOR[ \t]+${REQUIRED_OCCT_MAJOR}([^0-9]|$)" OR
   NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MINOR[ \t]+${REQUIRED_OCCT_MINOR}([^0-9]|$)" OR
   NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MAINTENANCE[ \t]+${REQUIRED_OCCT_MAINTENANCE}([^0-9]|$)")
    message(FATAL_ERROR "OcctCSharpBridge requires exactly OCCT ${REQUIRED_OCCT_VERSION}.")
endif()

add_library(OcctNative SHARED
    OcctEngine.cpp
    OcctNativeSurface.cpp
    OcctEngineView.cpp
    OcctRenderSurface.cpp
    OcctEngineSelection.cpp
    OcctEngineObjects.cpp
    OcctEngineShapes.cpp
    OcctAppearance.cpp
    OcctSelectionOverlay.cpp
    OcctView.cpp
    OcctViewportExtensions.cpp
    OcctViewportState.cpp
    OcctObjectBatch.cpp
    OcctObjectIdentity.cpp
    OcctObjectUpdate.cpp
    OcctObjectInteraction.cpp
    OcctObjectTransform.cpp
    OcctSelectionState.cpp
    OcctDepth.cpp
    OcctQueries.cpp
    OcctGeometry.cpp
    OcctAdvancedGeometry.cpp
    OcctFeatures.cpp
    OcctAnnotations.cpp
    OcctPoints.cpp
    OcctVectorAnnotations.cpp
    OcctIO.cpp
    OcctStepDocument.cpp
    OcctModelingCore.cpp
    OcctModelingShapeQueries.cpp
    OcctModelingGeometryQueries.cpp
    OcctModelingTopology.cpp
    OcctModelingInterop.cpp
    OcctModelingGeometry.Curves.cpp
    OcctModelingGeometry.Planar.cpp
    OcctModelingGeometry.Primitives.cpp
    OcctModelingGeometry.Assembly.cpp
    OcctModelingGeometry.Transform.cpp
    OcctModelingBoolean.cpp
    OcctModelingFeatures.cpp
    OcctModelingHealing.cpp
    OcctModelingHistory.cpp
    OcctModelingAnalysis.cpp
    OcctModelingMesh.cpp
    OcctModelingExchange.cpp
    OcctModelingAnalyticGeometry.cpp
    OcctModelingDifferentialGeometry.cpp
    OcctModelingExtensions.cpp
    OcctModelingBSpline.cpp
    OcctModelingTopologyAnalysis.cpp
    OcctModelingFaceAnalysis.cpp
    OcctModelingInertia.cpp
    OcctModelingIntersection.cpp
    OcctModelingTopologyReference.cpp
    OcctInternal.hxx
    OcctModelingSessionInternal.hxx
    OcctModelingShapeInternal.hxx
    OcctModelingAlgorithmInternal.hxx
    OcctModelingMeshInternal.hxx
    OcctModelingExchangeInternal.hxx
    OcctNative.h
    OcctNativeSurface.h
    OcctPoints.h
    OcctSelectionOverlay.h
    OcctSelectionState.h
    OcctModeling.h
    OcctModelingExtensions.h
    OcctModelingBSpline.h
    OcctModelingTopologyAnalysis.h
    OcctModelingFaceAnalysis.h
    OcctModelingInertia.h
    OcctModelingIntersection.h
    OcctModelingTopologyReference.h)

if(WIN32)
    target_compile_definitions(OcctNative PRIVATE NOMINMAX _CRT_SECURE_NO_WARNINGS)
endif()
if(MSVC)
    target_compile_options(OcctNative PRIVATE /utf-8 /W4 /WX /permissive- /external:W0)
elseif(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
    target_compile_options(OcctNative PRIVATE -Wall -Wextra -Wpedantic)
endif()

target_include_directories(OcctNative SYSTEM PRIVATE "${OCCT_INCLUDE_DIR}")
target_link_directories(OcctNative PRIVATE "${OCCT_LIB_DIR}")

set(occt_required_libraries
    TKernel TKMath TKG2d TKG3d TKGeomBase TKBRep TKGeomAlgo TKTopAlgo
    TKLCAF TKCAF TKXCAF
    TKPrim TKBO TKBool TKFillet TKOffset TKMesh TKShHealing
    TKService TKV3d TKOpenGl TKXSBase TKDE TKDESTEP TKDEIGES TKDESTL)

set(occt_link_libraries)
foreach(library_name IN LISTS occt_required_libraries)
    if(WIN32)
        if(NOT EXISTS "${OCCT_LIB_DIR}/${library_name}.lib")
            message(FATAL_ERROR "Required OCCT import library was not found: ${OCCT_LIB_DIR}/${library_name}.lib")
        endif()
        if(NOT EXISTS "${OCCT_BIN_DIR}/${library_name}.dll")
            message(FATAL_ERROR "Required OCCT runtime module was not found: ${OCCT_BIN_DIR}/${library_name}.dll")
        endif()
        list(APPEND occt_link_libraries ${library_name})
    else()
        find_library(OCCT_${library_name}_LIBRARY
            NAMES ${library_name}
            PATHS "${OCCT_LIB_DIR}"
            NO_DEFAULT_PATH)
        if(NOT OCCT_${library_name}_LIBRARY)
            message(FATAL_ERROR "Required OCCT library was not found in ${OCCT_LIB_DIR}: ${library_name}")
        endif()
        list(APPEND occt_link_libraries "${OCCT_${library_name}_LIBRARY}")
    endif()
endforeach()

message(STATUS "Bridge contract: ${BRIDGE_VERSION} (ABI ${BRIDGE_ABI_VERSION})")
message(STATUS "OCCT version: ${REQUIRED_OCCT_VERSION} (required exactly)")
message(STATUS "OCCT CASROOT: ${OCCT_CASROOT}")
message(STATUS "OCCT include: ${OCCT_INCLUDE_DIR}")
message(STATUS "OCCT lib: ${OCCT_LIB_DIR}")
if(WIN32)
    message(STATUS "OCCT bin: ${OCCT_BIN_DIR}")
endif()
message(STATUS "Required OCCT libraries: ${occt_required_libraries}")

if(WIN32)
    target_link_libraries(OcctNative PRIVATE ${occt_link_libraries} opengl32 user32 gdi32)
else()
    find_package(OpenGL REQUIRED)
    find_package(X11 REQUIRED)
    target_link_libraries(OcctNative PRIVATE ${occt_link_libraries} OpenGL::GL X11::X11)
    set_target_properties(OcctNative PROPERTIES
        BUILD_RPATH "${OCCT_LIB_DIR}"
        INSTALL_RPATH "$ORIGIN")
endif()

set_target_properties(OcctNative PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib")
foreach(configuration Debug Release RelWithDebInfo MinSizeRel)
    string(TOUPPER "${configuration}" configuration_upper)
    set_target_properties(OcctNative PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/bin/${configuration}"
        LIBRARY_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/bin/${configuration}"
        ARCHIVE_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/lib/${configuration}")
endforeach()

file(WRITE "${CMAKE_BINARY_DIR}/occt_paths.txt"
    "OCCT_CASROOT=${OCCT_CASROOT}\nOCCT_INCLUDE_DIR=${OCCT_INCLUDE_DIR}\nOCCT_LIB_DIR=${OCCT_LIB_DIR}\nOCCT_VERSION=${REQUIRED_OCCT_VERSION}\nBRIDGE_VERSION=${BRIDGE_VERSION}\nBRIDGE_ABI=${BRIDGE_ABI_VERSION}\n")