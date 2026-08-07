cmake_minimum_required(VERSION 3.21)

project(OcctNative LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS OFF)

set(OCCT_ROOT "$ENV{OCCT_ROOT}" CACHE PATH "OCCT installation root")
if(NOT OCCT_ROOT)
    message(FATAL_ERROR "OCCT_ROOT is not configured. Pass -DOCCT_ROOT=<path> or set the OCCT_ROOT environment variable.")
endif()
set(OCCT_INCLUDE_DIR "${OCCT_ROOT}/inc" CACHE PATH "OCCT include directory")
set(OCCT_LIB_DIR "${OCCT_ROOT}/win64/vc14/lib" CACHE PATH "OCCT library directory")
set(OCCT_BIN_DIR "${OCCT_ROOT}/win64/vc14/bin" CACHE PATH "OCCT runtime directory")
set(OCCT_CASROOT "${OCCT_ROOT}")

foreach(required_path
    "${OCCT_INCLUDE_DIR}/Standard.hxx"
    "${OCCT_INCLUDE_DIR}/Standard_Version.hxx"
    "${OCCT_LIB_DIR}/TKernel.lib"
    "${OCCT_BIN_DIR}/TKernel.dll")
    if(NOT EXISTS "${required_path}")
        message(FATAL_ERROR "Required OCCT file was not found: ${required_path}")
    endif()
endforeach()

file(READ "${OCCT_INCLUDE_DIR}/Standard_Version.hxx" _occt_version_header)
if(NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MAJOR[ \t]+7([^0-9]|$)" OR
   NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MINOR[ \t]+9([^0-9]|$)" OR
   NOT _occt_version_header MATCHES "#define[ \t]+OCC_VERSION_MAINTENANCE[ \t]+0([^0-9]|$)")
    message(FATAL_ERROR "OcctCSharpBridge requires exactly OCCT 7.9.0.")
endif()

add_library(OcctNative SHARED
    OcctEngine.cpp
    OcctAppearance.cpp
    OcctSelectionOverlay.cpp
    OcctView.cpp
    OcctViewportExtensions.cpp
    OcctViewportState.cpp
    OcctViewCube.cpp
    OcctObjectBatch.cpp
    OcctDepth.cpp
    OcctQueries.cpp
    OcctGeometry.cpp
    OcctAdvancedGeometry.cpp
    OcctFeatures.cpp
    OcctAnnotations.cpp
    OcctVectorAnnotations.cpp
    OcctIO.cpp
    OcctModelingCore.cpp
    OcctModelingGeometry.cpp
    OcctModelingAlgorithms.cpp
    OcctModelingAnalysis.cpp
    OcctModelingAnalyticGeometry.cpp
    OcctModelingDifferentialGeometry.cpp
    OcctInternal.hxx
    OcctModelingInternal.hxx
    OcctNative.h
    OcctSelectionOverlay.h
    OcctModeling.h)

target_compile_definitions(OcctNative PRIVATE NOMINMAX _CRT_SECURE_NO_WARNINGS)
if(MSVC)
    target_compile_options(OcctNative PRIVATE /utf-8 /W4 /WX /permissive- /external:W0)
endif()

target_include_directories(OcctNative SYSTEM PRIVATE "${OCCT_INCLUDE_DIR}")
target_link_directories(OcctNative PRIVATE "${OCCT_LIB_DIR}")

set(occt_candidate_libraries
    TKernel TKMath TKG2d TKG3d TKGeomBase TKBRep TKGeomAlgo TKTopAlgo
    TKPrim TKBO TKBool TKFillet TKOffset TKMesh TKShHealing
    TKService TKV3d TKOpenGl TKXSBase TKDE TKDESTEP TKDEIGES TKDESTL
    TKSTEPBase TKSTEPAttr TKSTEP209 TKSTEP TKIGES)

set(occt_libraries "")
foreach(library_name IN LISTS occt_candidate_libraries)
    if(EXISTS "${OCCT_LIB_DIR}/${library_name}.lib")
        list(APPEND occt_libraries "${library_name}")
    endif()
endforeach()

list(REMOVE_DUPLICATES occt_libraries)

message(STATUS "OCCT version: 7.9.0 (required exactly)")
message(STATUS "OCCT CASROOT: ${OCCT_CASROOT}")
message(STATUS "OCCT include: ${OCCT_INCLUDE_DIR}")
message(STATUS "OCCT lib: ${OCCT_LIB_DIR}")
message(STATUS "OCCT bin: ${OCCT_BIN_DIR}")
message(STATUS "OCCT libraries: ${occt_libraries}")

target_link_libraries(OcctNative PRIVATE ${occt_libraries} opengl32 user32 gdi32)

set_target_properties(OcctNative PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
    ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/lib")
foreach(configuration Debug Release RelWithDebInfo MinSizeRel)
    string(TOUPPER "${configuration}" configuration_upper)
    set_target_properties(OcctNative PROPERTIES
        RUNTIME_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/bin/${configuration}"
        ARCHIVE_OUTPUT_DIRECTORY_${configuration_upper} "${CMAKE_BINARY_DIR}/lib/${configuration}")
endforeach()

file(WRITE "${CMAKE_BINARY_DIR}/occt_paths.txt"
    "OCCT_CASROOT=${OCCT_CASROOT}\nOCCT_INCLUDE_DIR=${OCCT_INCLUDE_DIR}\nOCCT_LIB_DIR=${OCCT_LIB_DIR}\nOCCT_BIN_DIR=${OCCT_BIN_DIR}\nOCCT_VERSION=7.9.0\n")
