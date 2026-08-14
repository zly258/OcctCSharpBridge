#pragma once

#include "OcctStatus.h"

#include <string>
#include <utility>

namespace OcctBridge
{
    class ErrorContext
    {
    public:
        OcctStatus code = OcctStatus_Ok;
        std::string message;
        std::string scratch;

        void clear()
        {
            code = OcctStatus_Ok;
            message.clear();
            scratch.clear();
        }

        void set(OcctStatus status, std::string value)
        {
            code = status;
            message = std::move(value);
        }
    };
}
