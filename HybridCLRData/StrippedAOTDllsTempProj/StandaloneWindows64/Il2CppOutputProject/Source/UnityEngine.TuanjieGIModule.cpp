#include "pch-cpp.hpp"

#ifndef _MSC_VER
# include <alloca.h>
#else
# include <malloc.h>
#endif


#include <limits>



struct CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5;
struct Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184;
struct CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7;
struct RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27;
struct String_t;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct CameraCallback_t844E527BFE37BC0495E7F67993E43C07642DA9DD;

IL2CPP_EXTERN_C RuntimeField* ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var;
IL2CPP_EXTERN_C const RuntimeType* ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var;

struct CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B;
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248;
struct TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct U3CModuleU3E_t9565AAFE7C825CAD5321BA779F9B326DF6E3E007 
{
};
struct Manager_t022133EE08947149861116F85E734366237113FB  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	float ___m_value;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 
{
	float ___x;
	float ___y;
	float ___z;
	float ___w;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7  : public RuntimeObject
{
	intptr_t ___m_Ptr;
};
struct ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31 
{
	CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* ___commandBuffer;
	Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___camera;
	CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B* ___cameras;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___depthBufferBind;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___normalBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___motionBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___colorBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___cloudBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer0;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer1;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer2;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer3;
	uint32_t ___numOfDirectionalLights;
	uint32_t ___numOfOtherTypeLights;
	bool ___enableScreenTrace;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___directlightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___skylightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___emissivelightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___indirectlightingFactor;
	bool ___accurateGbufferNormals;
};
struct ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_pinvoke
{
	CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* ___commandBuffer;
	Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___camera;
	CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B* ___cameras;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___depthBufferBind;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___normalBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___motionBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___colorBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___cloudBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer0;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer1;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer2;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer3;
	uint32_t ___numOfDirectionalLights;
	uint32_t ___numOfOtherTypeLights;
	int32_t ___enableScreenTrace;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___directlightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___skylightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___emissivelightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___indirectlightingFactor;
	int32_t ___accurateGbufferNormals;
};
struct ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_com
{
	CommandBuffer_tB56007DC84EF56296C325EC32DD12AC1E3DC91F7* ___commandBuffer;
	Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___camera;
	CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B* ___cameras;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___depthBufferBind;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___normalBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___motionBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___colorBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___cloudBuffer;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer0;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer1;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer2;
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___GBuffer3;
	uint32_t ___numOfDirectionalLights;
	uint32_t ___numOfOtherTypeLights;
	int32_t ___enableScreenTrace;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___directlightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___skylightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___emissivelightingFactor;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___indirectlightingFactor;
	int32_t ___accurateGbufferNormals;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C  : public RuntimeObject
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_pinvoke
{
	intptr_t ___m_CachedPtr;
};
struct Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C_marshaled_com
{
	intptr_t ___m_CachedPtr;
};
struct PipelineType_tA465FDAAED9088C9338DE4B4914FE0EFB10F2B3D 
{
	int32_t ___value__;
};
struct Type_t072E62E5CF6157038AA33E20BCA165B62207B061 
{
	int32_t ___value__;
};
struct Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct TuanjieGIConfigObject_tFDF0A975677D72ED37BE948D8F15FBA4A6F6FF0D  : public Object_tC12DECB6760A7F2CBF65D9DCF18D044C2D97152C
{
};
struct TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE 
{
	int32_t ___type;
	String_t* ___name;
	String_t* ___desc;
	int32_t ___id;
};
struct TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_pinvoke
{
	int32_t ___type;
	char* ___name;
	char* ___desc;
	int32_t ___id;
};
struct TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_com
{
	int32_t ___type;
	Il2CppChar* ___name;
	Il2CppChar* ___desc;
	int32_t ___id;
};
struct Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA  : public Component_t39FBE53E5EFCF4409111FB22C15FF73717632EC3
{
};
struct RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27  : public Texture_t791CBB51219779964E0E8A2ED7C1AA5F92A4A700
{
};
struct Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184  : public Behaviour_t01970CFBBA658497AE30F311C447DB0440BAB7FA
{
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3_StaticFields
{
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___zeroVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___oneVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___positiveInfinityVector;
	Vector4_t58B63D32F48C0DBF50DE2C60794C4676C80EDBE3 ___negativeInfinityVector;
};
struct Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184_StaticFields
{
	CameraCallback_t844E527BFE37BC0495E7F67993E43C07642DA9DD* ___onPreCull;
	CameraCallback_t844E527BFE37BC0495E7F67993E43C07642DA9DD* ___onPreRender;
	CameraCallback_t844E527BFE37BC0495E7F67993E43C07642DA9DD* ___onPostRender;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248  : public RuntimeArray
{
	ALIGN_FIELD (8) String_t* m_Items[1];

	inline String_t* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline String_t** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, String_t* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline String_t* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline String_t** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, String_t* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5  : public RuntimeArray
{
	ALIGN_FIELD (8) TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE m_Items[1];

	inline TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___name), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___desc), (void*)NULL);
		#endif
	}
	inline TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___name), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___desc), (void*)NULL);
		#endif
	}
};
struct CameraU5BU5D_t1506EBA524A07AD1066D6DD4D7DFC6721F1AC26B  : public RuntimeArray
{
	ALIGN_FIELD (8) Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* m_Items[1];

	inline Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};



IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC (ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31* ___0_ectx, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247 (RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___0_renderTarget, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31* ___1_ectx, const RuntimeMethod* method) ;
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Manager_get_debugTextureIndex_m5183147CC660C5BDB418524F5227B73D864590F4 (const RuntimeMethod* method) 
{
	typedef int32_t (*Manager_get_debugTextureIndex_m5183147CC660C5BDB418524F5227B73D864590F4_ftn) ();
	static Manager_get_debugTextureIndex_m5183147CC660C5BDB418524F5227B73D864590F4_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_get_debugTextureIndex_m5183147CC660C5BDB418524F5227B73D864590F4_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::get_debugTextureIndex()");
	int32_t icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Manager_set_debugTextureIndex_mF781481CD3182ABD85DB616604DCFB73E8FD44D2 (int32_t ___0_value, const RuntimeMethod* method) 
{
	typedef void (*Manager_set_debugTextureIndex_mF781481CD3182ABD85DB616604DCFB73E8FD44D2_ftn) (int32_t);
	static Manager_set_debugTextureIndex_mF781481CD3182ABD85DB616604DCFB73E8FD44D2_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_set_debugTextureIndex_mF781481CD3182ABD85DB616604DCFB73E8FD44D2_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::set_debugTextureIndex(System.Int32)");
	_il2cpp_icall_func(___0_value);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Manager_get_pipelineType_m28798CDF87E24BB3556CF5657B8B9EE89089C793 (const RuntimeMethod* method) 
{
	typedef int32_t (*Manager_get_pipelineType_m28798CDF87E24BB3556CF5657B8B9EE89089C793_ftn) ();
	static Manager_get_pipelineType_m28798CDF87E24BB3556CF5657B8B9EE89089C793_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_get_pipelineType_m28798CDF87E24BB3556CF5657B8B9EE89089C793_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::get_pipelineType()");
	int32_t icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Manager_set_pipelineType_m5867B13F7E2BB80AD249654911EAC7C644A632AE (int32_t ___0_value, const RuntimeMethod* method) 
{
	typedef void (*Manager_set_pipelineType_m5867B13F7E2BB80AD249654911EAC7C644A632AE_ftn) (int32_t);
	static Manager_set_pipelineType_m5867B13F7E2BB80AD249654911EAC7C644A632AE_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_set_pipelineType_m5867B13F7E2BB80AD249654911EAC7C644A632AE_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::set_pipelineType(UnityEngine.TuanjieGI.PipelineType)");
	_il2cpp_icall_func(___0_value);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Manager_get_fullscreenDebugMinRange_mA4F481C1BAC4EDDD4202CCF62C62481ECEF9480F (const RuntimeMethod* method) 
{
	typedef float (*Manager_get_fullscreenDebugMinRange_mA4F481C1BAC4EDDD4202CCF62C62481ECEF9480F_ftn) ();
	static Manager_get_fullscreenDebugMinRange_mA4F481C1BAC4EDDD4202CCF62C62481ECEF9480F_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_get_fullscreenDebugMinRange_mA4F481C1BAC4EDDD4202CCF62C62481ECEF9480F_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::get_fullscreenDebugMinRange()");
	float icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Manager_set_fullscreenDebugMinRange_m30D157274760F98DE5A8E8A7E3118643858962CE (float ___0_value, const RuntimeMethod* method) 
{
	typedef void (*Manager_set_fullscreenDebugMinRange_m30D157274760F98DE5A8E8A7E3118643858962CE_ftn) (float);
	static Manager_set_fullscreenDebugMinRange_m30D157274760F98DE5A8E8A7E3118643858962CE_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_set_fullscreenDebugMinRange_m30D157274760F98DE5A8E8A7E3118643858962CE_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::set_fullscreenDebugMinRange(System.Single)");
	_il2cpp_icall_func(___0_value);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float Manager_get_fullscreenDebugMaxRange_m25FCD65086211C65C1A39F3CA6CD4939F39B629B (const RuntimeMethod* method) 
{
	typedef float (*Manager_get_fullscreenDebugMaxRange_m25FCD65086211C65C1A39F3CA6CD4939F39B629B_ftn) ();
	static Manager_get_fullscreenDebugMaxRange_m25FCD65086211C65C1A39F3CA6CD4939F39B629B_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_get_fullscreenDebugMaxRange_m25FCD65086211C65C1A39F3CA6CD4939F39B629B_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::get_fullscreenDebugMaxRange()");
	float icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Manager_set_fullscreenDebugMaxRange_m4155CD8A43B259FE98E52C82965A03925274D2B8 (float ___0_value, const RuntimeMethod* method) 
{
	typedef void (*Manager_set_fullscreenDebugMaxRange_m4155CD8A43B259FE98E52C82965A03925274D2B8_ftn) (float);
	static Manager_set_fullscreenDebugMaxRange_m4155CD8A43B259FE98E52C82965A03925274D2B8_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_set_fullscreenDebugMaxRange_m4155CD8A43B259FE98E52C82965A03925274D2B8_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::set_fullscreenDebugMaxRange(System.Single)");
	_il2cpp_icall_func(___0_value);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Manager_set_fullscreenDebugComputeRangeTag_mD9F682FB447533F8997B9BEFF1A1BCDFBC69E3A4 (bool ___0_value, const RuntimeMethod* method) 
{
	typedef void (*Manager_set_fullscreenDebugComputeRangeTag_mD9F682FB447533F8997B9BEFF1A1BCDFBC69E3A4_ftn) (bool);
	static Manager_set_fullscreenDebugComputeRangeTag_mD9F682FB447533F8997B9BEFF1A1BCDFBC69E3A4_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_set_fullscreenDebugComputeRangeTag_mD9F682FB447533F8997B9BEFF1A1BCDFBC69E3A4_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::set_fullscreenDebugComputeRangeTag(System.Boolean)");
	_il2cpp_icall_func(___0_value);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* Manager_get_pooledTextureNames_mAF78851C9758DC6D6CAC13310BCE71680B183C71 (const RuntimeMethod* method) 
{
	typedef StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* (*Manager_get_pooledTextureNames_mAF78851C9758DC6D6CAC13310BCE71680B183C71_ftn) ();
	static Manager_get_pooledTextureNames_mAF78851C9758DC6D6CAC13310BCE71680B183C71_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_get_pooledTextureNames_mAF78851C9758DC6D6CAC13310BCE71680B183C71_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::get_pooledTextureNames()");
	StringU5BU5D_t7674CD946EC0CE7B3AE0BE70E6EE85F2ECD9F248* icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* Manager_GetTextureByIndex_m3314CFF7C7973F0739C4948928496A7941EDAC73 (int32_t ___0_index, const RuntimeMethod* method) 
{
	typedef RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* (*Manager_GetTextureByIndex_m3314CFF7C7973F0739C4948928496A7941EDAC73_ftn) (int32_t);
	static Manager_GetTextureByIndex_m3314CFF7C7973F0739C4948928496A7941EDAC73_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (Manager_GetTextureByIndex_m3314CFF7C7973F0739C4948928496A7941EDAC73_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.Manager::GetTextureByIndex(System.Int32)");
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* icallRetVal = _il2cpp_icall_func(___0_index);
	return icallRetVal;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_pinvoke(const TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE& unmarshaled, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_pinvoke& marshaled)
{
	marshaled.___type = unmarshaled.___type;
	marshaled.___name = il2cpp_codegen_marshal_string(unmarshaled.___name);
	marshaled.___desc = il2cpp_codegen_marshal_string(unmarshaled.___desc);
	marshaled.___id = unmarshaled.___id;
}
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_pinvoke_back(const TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_pinvoke& marshaled, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE& unmarshaled)
{
	int32_t unmarshaledtype_temp_0 = 0;
	unmarshaledtype_temp_0 = marshaled.___type;
	unmarshaled.___type = unmarshaledtype_temp_0;
	unmarshaled.___name = il2cpp_codegen_marshal_string_result(marshaled.___name);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___name), (void*)il2cpp_codegen_marshal_string_result(marshaled.___name));
	unmarshaled.___desc = il2cpp_codegen_marshal_string_result(marshaled.___desc);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___desc), (void*)il2cpp_codegen_marshal_string_result(marshaled.___desc));
	int32_t unmarshaledid_temp_3 = 0;
	unmarshaledid_temp_3 = marshaled.___id;
	unmarshaled.___id = unmarshaledid_temp_3;
}
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_pinvoke_cleanup(TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_pinvoke& marshaled)
{
	il2cpp_codegen_marshal_free(marshaled.___name);
	marshaled.___name = NULL;
	il2cpp_codegen_marshal_free(marshaled.___desc);
	marshaled.___desc = NULL;
}
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_com(const TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE& unmarshaled, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_com& marshaled)
{
	marshaled.___type = unmarshaled.___type;
	marshaled.___name = il2cpp_codegen_marshal_bstring(unmarshaled.___name);
	marshaled.___desc = il2cpp_codegen_marshal_bstring(unmarshaled.___desc);
	marshaled.___id = unmarshaled.___id;
}
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_com_back(const TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_com& marshaled, TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE& unmarshaled)
{
	int32_t unmarshaledtype_temp_0 = 0;
	unmarshaledtype_temp_0 = marshaled.___type;
	unmarshaled.___type = unmarshaledtype_temp_0;
	unmarshaled.___name = il2cpp_codegen_marshal_bstring_result(marshaled.___name);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___name), (void*)il2cpp_codegen_marshal_bstring_result(marshaled.___name));
	unmarshaled.___desc = il2cpp_codegen_marshal_bstring_result(marshaled.___desc);
	Il2CppCodeGenWriteBarrier((void**)(&unmarshaled.___desc), (void*)il2cpp_codegen_marshal_bstring_result(marshaled.___desc));
	int32_t unmarshaledid_temp_3 = 0;
	unmarshaledid_temp_3 = marshaled.___id;
	unmarshaled.___id = unmarshaledid_temp_3;
}
IL2CPP_EXTERN_C void TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshal_com_cleanup(TuanjieGIOption_t3A78B9C068FCE3C5A2A158F2850D9518FDC265CE_marshaled_com& marshaled)
{
	il2cpp_codegen_marshal_free_bstring(marshaled.___name);
	marshaled.___name = NULL;
	il2cpp_codegen_marshal_free_bstring(marshaled.___desc);
	marshaled.___desc = NULL;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* TuanjieGIOption_MarshalOptions_m413AE492807390622851277D7E74685E952FEEE7 (const RuntimeMethod* method) 
{
	typedef TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* (*TuanjieGIOption_MarshalOptions_m413AE492807390622851277D7E74685E952FEEE7_ftn) ();
	static TuanjieGIOption_MarshalOptions_m413AE492807390622851277D7E74685E952FEEE7_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_MarshalOptions_m413AE492807390622851277D7E74685E952FEEE7_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::MarshalOptions()");
	TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* TuanjieGIOption_MarshalVisualizeOptions_m8194612B1BB0A00B684D387B1761C0441FD48734 (const RuntimeMethod* method) 
{
	typedef TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* (*TuanjieGIOption_MarshalVisualizeOptions_m8194612B1BB0A00B684D387B1761C0441FD48734_ftn) ();
	static TuanjieGIOption_MarshalVisualizeOptions_m8194612B1BB0A00B684D387B1761C0441FD48734_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_MarshalVisualizeOptions_m8194612B1BB0A00B684D387B1761C0441FD48734_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::MarshalVisualizeOptions()");
	TuanjieGIOptionU5BU5D_t7D94F9531A50F38EA5AD340606B8C313E160C2C5* icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void TuanjieGIOption_SetVisualizeOption_m04839D235CC1C517CE73713E7BFAFEE4EC692969 (int32_t ___0_id, bool ___1_option, const RuntimeMethod* method) 
{
	typedef void (*TuanjieGIOption_SetVisualizeOption_m04839D235CC1C517CE73713E7BFAFEE4EC692969_ftn) (int32_t, bool);
	static TuanjieGIOption_SetVisualizeOption_m04839D235CC1C517CE73713E7BFAFEE4EC692969_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_SetVisualizeOption_m04839D235CC1C517CE73713E7BFAFEE4EC692969_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::SetVisualizeOption(System.Int32,System.Boolean)");
	_il2cpp_icall_func(___0_id, ___1_option);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void TuanjieGIOption_SetBoolOption_mFCBA1B29D9BC1AF9F72C8D94066B256BA3D05A4D (int32_t ___0_id, bool ___1_option, const RuntimeMethod* method) 
{
	typedef void (*TuanjieGIOption_SetBoolOption_mFCBA1B29D9BC1AF9F72C8D94066B256BA3D05A4D_ftn) (int32_t, bool);
	static TuanjieGIOption_SetBoolOption_mFCBA1B29D9BC1AF9F72C8D94066B256BA3D05A4D_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_SetBoolOption_mFCBA1B29D9BC1AF9F72C8D94066B256BA3D05A4D_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::SetBoolOption(System.Int32,System.Boolean)");
	_il2cpp_icall_func(___0_id, ___1_option);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool TuanjieGIOption_GetBoolOption_m0EBD3696D68E45BFC33C812977B8E000C36B1468 (int32_t ___0_id, const RuntimeMethod* method) 
{
	typedef bool (*TuanjieGIOption_GetBoolOption_m0EBD3696D68E45BFC33C812977B8E000C36B1468_ftn) (int32_t);
	static TuanjieGIOption_GetBoolOption_m0EBD3696D68E45BFC33C812977B8E000C36B1468_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_GetBoolOption_m0EBD3696D68E45BFC33C812977B8E000C36B1468_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::GetBoolOption(System.Int32)");
	bool icallRetVal = _il2cpp_icall_func(___0_id);
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void TuanjieGIOption_SetIntOption_mE28C9162F57E3FC665B4FE64E8649F97135BFBAE (int32_t ___0_id, int32_t ___1_option, const RuntimeMethod* method) 
{
	typedef void (*TuanjieGIOption_SetIntOption_mE28C9162F57E3FC665B4FE64E8649F97135BFBAE_ftn) (int32_t, int32_t);
	static TuanjieGIOption_SetIntOption_mE28C9162F57E3FC665B4FE64E8649F97135BFBAE_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_SetIntOption_mE28C9162F57E3FC665B4FE64E8649F97135BFBAE_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::SetIntOption(System.Int32,System.Int32)");
	_il2cpp_icall_func(___0_id, ___1_option);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t TuanjieGIOption_GetIntOption_m9FE1FC9CDD91CCA39A8ADFBFFA0C3101DB5B9EA1 (int32_t ___0_id, const RuntimeMethod* method) 
{
	typedef int32_t (*TuanjieGIOption_GetIntOption_m9FE1FC9CDD91CCA39A8ADFBFFA0C3101DB5B9EA1_ftn) (int32_t);
	static TuanjieGIOption_GetIntOption_m9FE1FC9CDD91CCA39A8ADFBFFA0C3101DB5B9EA1_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_GetIntOption_m9FE1FC9CDD91CCA39A8ADFBFFA0C3101DB5B9EA1_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::GetIntOption(System.Int32)");
	int32_t icallRetVal = _il2cpp_icall_func(___0_id);
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void TuanjieGIOption_SetFloatOption_m5026C34C30C83473946E2B38EB4989C52B384315 (int32_t ___0_id, float ___1_option, const RuntimeMethod* method) 
{
	typedef void (*TuanjieGIOption_SetFloatOption_m5026C34C30C83473946E2B38EB4989C52B384315_ftn) (int32_t, float);
	static TuanjieGIOption_SetFloatOption_m5026C34C30C83473946E2B38EB4989C52B384315_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_SetFloatOption_m5026C34C30C83473946E2B38EB4989C52B384315_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::SetFloatOption(System.Int32,System.Single)");
	_il2cpp_icall_func(___0_id, ___1_option);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float TuanjieGIOption_GetFloatOption_m7B9CDB8A63852688F88A213B1F4A93900151C717 (int32_t ___0_id, const RuntimeMethod* method) 
{
	typedef float (*TuanjieGIOption_GetFloatOption_m7B9CDB8A63852688F88A213B1F4A93900151C717_ftn) (int32_t);
	static TuanjieGIOption_GetFloatOption_m7B9CDB8A63852688F88A213B1F4A93900151C717_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (TuanjieGIOption_GetFloatOption_m7B9CDB8A63852688F88A213B1F4A93900151C717_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.TuanjieGIOption::GetFloatOption(System.Int32)");
	float icallRetVal = _il2cpp_icall_func(___0_id);
	return icallRetVal;
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
#ifdef __clang__
#pragma clang diagnostic pop
#endif
#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_pinvoke(const ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31& unmarshaled, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_pinvoke& marshaled)
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Exception_t* ___commandBufferException = il2cpp_codegen_get_marshal_directive_exception("Cannot marshal field '%s' of type '%s': Reference type field marshaling is not supported.", ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
	IL2CPP_RAISE_MANAGED_EXCEPTION(___commandBufferException, NULL);
}
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_pinvoke_back(const ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_pinvoke& marshaled, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31& unmarshaled)
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Exception_t* ___commandBufferException = il2cpp_codegen_get_marshal_directive_exception("Cannot marshal field '%s' of type '%s': Reference type field marshaling is not supported.", ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
	IL2CPP_RAISE_MANAGED_EXCEPTION(___commandBufferException, NULL);
}
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_pinvoke_cleanup(ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_pinvoke& marshaled)
{
}
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_com(const ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31& unmarshaled, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_com& marshaled)
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Exception_t* ___commandBufferException = il2cpp_codegen_get_marshal_directive_exception("Cannot marshal field '%s' of type '%s': Reference type field marshaling is not supported.", ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
	IL2CPP_RAISE_MANAGED_EXCEPTION(___commandBufferException, NULL);
}
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_com_back(const ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_com& marshaled, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31& unmarshaled)
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	Exception_t* ___commandBufferException = il2cpp_codegen_get_marshal_directive_exception("Cannot marshal field '%s' of type '%s': Reference type field marshaling is not supported.", ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31____commandBuffer_FieldInfo_var, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_0_0_0_var);
	IL2CPP_RAISE_MANAGED_EXCEPTION(___commandBufferException, NULL);
}
IL2CPP_EXTERN_C void ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshal_com_cleanup(ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31_marshaled_com& marshaled)
{
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_Execute_m28960424B4D81E6E0B4552D70A095E3A501D7B5B (ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31 ___0_ectx, const RuntimeMethod* method) 
{
	{
		ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC((&___0_ectx), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ExecutionContextURP_GetDiffuseIndirectTexture_m7109CAB384ED0B24A335F75ECCADC1D7E5F69724 (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___0_camera, const RuntimeMethod* method) 
{
	typedef RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* (*ExecutionContextURP_GetDiffuseIndirectTexture_m7109CAB384ED0B24A335F75ECCADC1D7E5F69724_ftn) (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184*);
	static ExecutionContextURP_GetDiffuseIndirectTexture_m7109CAB384ED0B24A335F75ECCADC1D7E5F69724_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_GetDiffuseIndirectTexture_m7109CAB384ED0B24A335F75ECCADC1D7E5F69724_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::GetDiffuseIndirectTexture(UnityEngine.Camera)");
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* icallRetVal = _il2cpp_icall_func(___0_camera);
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ExecutionContextURP_GetSpecularIndirectTexture_mC77C125D2CA6CEAFF090EA2AC3284E50D1CE4EFF (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___0_camera, const RuntimeMethod* method) 
{
	typedef RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* (*ExecutionContextURP_GetSpecularIndirectTexture_mC77C125D2CA6CEAFF090EA2AC3284E50D1CE4EFF_ftn) (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184*);
	static ExecutionContextURP_GetSpecularIndirectTexture_mC77C125D2CA6CEAFF090EA2AC3284E50D1CE4EFF_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_GetSpecularIndirectTexture_mC77C125D2CA6CEAFF090EA2AC3284E50D1CE4EFF_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::GetSpecularIndirectTexture(UnityEngine.Camera)");
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* icallRetVal = _il2cpp_icall_func(___0_camera);
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ExecutionContextURP_GetRoughSpecularIndirectTexture_mD9FE38A37152EDA4867EC88B51D170AFBC094C61 (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184* ___0_camera, const RuntimeMethod* method) 
{
	typedef RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* (*ExecutionContextURP_GetRoughSpecularIndirectTexture_mD9FE38A37152EDA4867EC88B51D170AFBC094C61_ftn) (Camera_tA92CC927D7439999BC82DBEDC0AA45B470F9E184*);
	static ExecutionContextURP_GetRoughSpecularIndirectTexture_mD9FE38A37152EDA4867EC88B51D170AFBC094C61_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_GetRoughSpecularIndirectTexture_mD9FE38A37152EDA4867EC88B51D170AFBC094C61_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::GetRoughSpecularIndirectTexture(UnityEngine.Camera)");
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* icallRetVal = _il2cpp_icall_func(___0_camera);
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_DrawFullScreenDebug_m2A38AB92E44EAF72E79FE3DBA7A2570BEA068B5B (RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___0_renderTarget, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31 ___1_ectx, const RuntimeMethod* method) 
{
	{
		RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* L_0 = ___0_renderTarget;
		ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247(L_0, (&___1_ectx), NULL);
		return;
	}
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ExecutionContextURP_GetSkyboxCubemap_m7E7AA630C51322FECC0ACA96D7D0F475AB097EA4 (const RuntimeMethod* method) 
{
	typedef RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* (*ExecutionContextURP_GetSkyboxCubemap_m7E7AA630C51322FECC0ACA96D7D0F475AB097EA4_ftn) ();
	static ExecutionContextURP_GetSkyboxCubemap_m7E7AA630C51322FECC0ACA96D7D0F475AB097EA4_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_GetSkyboxCubemap_m7E7AA630C51322FECC0ACA96D7D0F475AB097EA4_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::GetSkyboxCubemap()");
	RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* icallRetVal = _il2cpp_icall_func();
	return icallRetVal;
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC (ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31* ___0_ectx, const RuntimeMethod* method) 
{
	typedef void (*ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC_ftn) (ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31*);
	static ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_Execute_Injected_m7DE1AA4F4010B1FB011FFA540821D1CE827F8FAC_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::Execute_Injected(UnityEngine.TuanjieGI.ExecutionContextURP&)");
	_il2cpp_icall_func(___0_ectx);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247 (RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27* ___0_renderTarget, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31* ___1_ectx, const RuntimeMethod* method) 
{
	typedef void (*ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247_ftn) (RenderTexture_tBA90C4C3AD9EECCFDDCC632D97C29FAB80D60D27*, ExecutionContextURP_tD49DC2476E6214728015D06BC8345902EEF3CB31*);
	static ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247_ftn _il2cpp_icall_func;
	if (!_il2cpp_icall_func)
	_il2cpp_icall_func = (ExecutionContextURP_DrawFullScreenDebug_Injected_m6F384EBCADF8F508B224A4D9D5D37F890F233247_ftn)il2cpp_codegen_resolve_icall ("UnityEngine.TuanjieGI.ExecutionContextURP::DrawFullScreenDebug_Injected(UnityEngine.RenderTexture,UnityEngine.TuanjieGI.ExecutionContextURP&)");
	_il2cpp_icall_func(___0_renderTarget, ___1_ectx);
}
#ifdef __clang__
#pragma clang diagnostic pop
#endif
