#pragma once
#include "OcctStatus.h"
#include <Standard_Failure.hxx>
#include <stdexcept>
#include <new>

// 用于持有 Engine* 的函数，errors 写入 engine->setError
#define OCCT_CATCH_ENGINE(engine, retval)                                          \
    catch (const Standard_Failure& f) {                                            \
        if (engine) engine->setError(OcctStatus_ErrorOcct, f.GetMessageString()   \
            ? f.GetMessageString() : "Open CASCADE operation failed.");           \
        return retval;                                                             \
    }                                                                              \
    catch (const std::invalid_argument& e) {                                       \
        if (engine) engine->setError(OcctStatus_ErrorInvalidArgument, e.what());  \
        return retval;                                                             \
    }                                                                              \
    catch (const std::bad_alloc&) {                                                \
        if (engine) engine->setError(OcctStatus_ErrorOutOfMemory,                  \
            "Native memory allocation failed.");                                   \
        return retval;                                                             \
    }                                                                              \
    catch (const std::exception& e) {                                              \
        if (engine) engine->setError(OcctStatus_ErrorUnknown, e.what());          \
        return retval;                                                             \
    }                                                                              \
    catch (...) {                                                                  \
        if (engine) engine->setError(OcctStatus_ErrorUnknown,                      \
            "Unknown native error.");                                              \
        return retval;                                                             \
    }

// 用于持有 Session* 的函数，errors 写入 session->setError
#define OCCT_CATCH_SESSION(session, retval)                                        \
    catch (const Standard_Failure& f) {                                            \
        if (session) session->setError(OcctStatus_ErrorOcct, f.GetMessageString()  \
            ? f.GetMessageString() : "Open CASCADE operation failed.");           \
        return retval;                                                             \
    }                                                                              \
    catch (const std::invalid_argument& e) {                                       \
        if (session) session->setError(OcctStatus_ErrorInvalidArgument, e.what());\
        return retval;                                                             \
    }                                                                              \
    catch (const std::bad_alloc&) {                                                \
        if (session) session->setError(OcctStatus_ErrorOutOfMemory,                \
            "Native memory allocation failed.");                                   \
        return retval;                                                             \
    }                                                                              \
    catch (const std::exception& e) {                                              \
        if (session) session->setError(OcctStatus_ErrorUnknown, e.what());         \
        return retval;                                                             \
    }                                                                              \
    catch (...) {                                                                  \
        if (session) session->setError(OcctStatus_ErrorUnknown,                    \
            "Unknown native error.");                                              \
        return retval;                                                             \
    }
