using System;

namespace F4SharedMem.Headers
{	// BLINKING LIGHTS - only indicating *IF* a lamp is blinking, not implementing the actual on/off/blinking pattern logic!
    [Flags]
    [Serializable]
    public enum JammingStates : byte
    {
        JAMMED_NO     = 0,
        JAMMED_YES    = 1,
        JAMMED_SHOULD = 2,
    };
}
