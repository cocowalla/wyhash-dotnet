#include <stdio.h>
#include "wyhash.h"

extern "C"
{
    __declspec(dllexport) unsigned long long WyHash64(char* data, unsigned long long count, unsigned long long seed)
    {
		unsigned long long result = wyhash(data, count, seed);
		return result;
    }
}
