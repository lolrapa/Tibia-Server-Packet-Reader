//Packet Reader DLL / Function Hook DLL BY LolRapa [special thanks to DarkstaR]
//This dll allows you to set your own function before the executon of a tibia function
//If you want to hook any other function simply replace sendPacket for the address and the behaviour in myFunction()
//This dll provides a comunication buffer via FileMapping, packetReaderMemoryFile must be open in loader in order to communicate
//I hope you enjoy this dll!!
#include "stdafx.h"
#include <Windows.h>
#include <stdlib.h>
#include <fstream>
void __stdcall myFunction();
typedef void(__stdcall *_miFunc)();
LPVOID filePointer;

//This addresses are for tibia 10.78 and must be updated in order to work
LPVOID sendPacket = (LPVOID)0x541810;
//DWORD* SendStreamData = (DWORD*)0x848B30;
LPVOID SendStreamData = reinterpret_cast<LPVOID>(0x848B30);

DWORD* SendStreamLength = reinterpret_cast<DWORD*>(0xA76B10);
//





DWORD WINAPI Attach(LPVOID lpParameters)
{
	//File map
	HANDLE fileHandle = CreateFileMappingA(INVALID_HANDLE_VALUE, NULL, PAGE_READWRITE, 0, 1024, "packetReaderMemoryFile");
	filePointer = MapViewOfFile(fileHandle, FILE_MAP_WRITE, 0, 0, 0);
	*(BYTE*)filePointer = 0;
	//


	
	_miFunc ptrMyFunc = (_miFunc)myFunction; //get a pointer to my function

	DWORD addrSendPacket = (DWORD)sendPacket; //get the address of sendpacket pointer
	DWORD addrMyFunc = (DWORD)ptrMyFunc; //get the address to my function
	addrMyFunc = addrMyFunc - (addrSendPacket + 5); //calculate offset
	DWORD oldProtect;
	VirtualProtect((void*)addrSendPacket, 5, PAGE_READWRITE, &oldProtect);
	*(BYTE*)addrSendPacket = 0xE8; //CALL
	*(DWORD*)((LPVOID)(&((CHAR*)addrSendPacket)[1])) = addrMyFunc; //my function addres
	VirtualProtect((void*)addrSendPacket, 5, oldProtect, &oldProtect);

	return 1;
}




void __stdcall myFunction()
{
		// This function will be called when tibia calls sendpacket I deliver the packet info to the loader via file mapping

		*(DWORD*)((LPVOID)(&((CHAR*)filePointer)[1])) = *SendStreamLength; //write the data len in 2nd byte (four bytes)
		memcpy((LPVOID)(&((CHAR*)filePointer)[5]), SendStreamData, *SendStreamLength); //write the data buffer in 5th byte (sendStreamLenght bytes)
		*(BYTE*)filePointer = 1; //inticates to the loader there's been an update, loader must set this byte to 0 after reading

}




BOOL APIENTRY DllMain(HMODULE hModule, DWORD  ul_reason_for_call, LPVOID lpReserved)
{
	if (ul_reason_for_call == DLL_PROCESS_ATTACH)
	{
		HANDLE th = CreateThread(NULL, 0, Attach, NULL, 0, 0);
		CloseHandle(th);
	}
	else if (ul_reason_for_call == DLL_PROCESS_DETACH)
	{
		//free(filePointer);
	}
	return TRUE;
}

