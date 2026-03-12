using Il2CppInterop.Runtime;

namespace KogamaToolsEx.Helpers
{
    internal class Il2CppEnum<T> : Il2CppSystem.Enum where T : unmanaged, Enum
    {
        public Il2CppEnum(IntPtr pointer) : base(pointer) { }

        public Il2CppEnum() : base(IL2CPP.il2cpp_object_new(Il2CppClassPointerStore<T>.NativeClassPtr)) { }

        public unsafe Il2CppEnum(T value) : base(IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<T>.NativeClassPtr, new IntPtr(&value))) { }

        public unsafe T Value
        {
            get
            {
                var ptr = IL2CPP.il2cpp_object_unbox(Pointer);
                return *(T*)ptr;
            }
            set
            {
                var ptr = IL2CPP.il2cpp_object_unbox(Pointer);
                *(T*)ptr = value;
            }
        }

        public static implicit operator T(Il2CppEnum<T> value) => value.Value;
        public static implicit operator Il2CppEnum<T>(T value) => new(value);
    }
}
