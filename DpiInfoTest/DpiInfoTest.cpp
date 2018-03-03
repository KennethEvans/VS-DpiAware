// DpiInfoTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <iostream>
using namespace std;

// Function prototypes
BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam);
DPI_AWARENESS GetProcessDpiAwareness(DWORD processID);
void PrintProcessNameAndID(DWORD processID);

int main()
{
	cout << "Starting DpiInfoTest\n\n";

	EnumWindows(EnumWindowsProc, NULL);

	//HWND hWnd = GetActiveWindow();
	//DPI_AWARENESS_CONTEXT context = GetWindowDpiAwarenessContext(hWnd);
	//cout << "hWnd=" << hWnd << endl;
	//cout << "context=" << context << endl;

	return 0;
}

BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam){
	TCHAR className[512];
	TCHAR title[512];
	TCHAR baseName[MAX_PATH];
	int bytesClassName = GetClassName(hWnd, className, sizeof(className) / sizeof(TCHAR));
	int bytesWindowText = GetWindowText(hWnd, title, sizeof(title) / sizeof(TCHAR));
	int dpi = GetDpiForWindow(hWnd);
	DWORD processID;
	GetWindowThreadProcessId(hWnd, &processID);
	DPI_AWARENESS dpiAwareness = DPI_AWARENESS_INVALID;
	//dpiAwareness = GetProcessDpiAwareness(processID);
	DPI_AWARENESS_CONTEXT context = GetWindowDpiAwarenessContext(hWnd);
	//className[bytesClassName] = _T('\0');
	//title[bytesWindowText] = _T('\0');
	//moduleFileName[bytesModuleFileName] = _T('\0');
	wcout << "hWnd: " << hWnd << endl;
	wcout << "PID: " << processID << endl;
	PrintProcessNameAndID(processID);
	wcout << "Window Title: [" << bytesWindowText << "]  " << title << endl;
	wcout << "Class name: [" << bytesClassName << "]  " << className << endl;
	wcout << "DPI: " << dpi << endl;
	wcout << "DPI Awareness: " << dpiAwareness << endl;
	wcout << "DPI_AWARENESS_CONTEXT: " << context << endl << endl;

	return TRUE;
}

DPI_AWARENESS GetProcessDpiAwareness(DWORD processID) {
	DPI_AWARENESS dpiAwareness;
	int dpiAwarenessErr = 0;
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION, false, processID);
	if (hProcess != NULL) {
		SetLastError(0);
		dpiAwareness = GetProcessDpiAwareness(processID);
		dpiAwarenessErr = GetLastError();
		CloseHandle(hProcess);
	} else {
		dpiAwareness = DPI_AWARENESS_INVALID;
	}
	return dpiAwareness;
}

void PrintProcessNameAndID(DWORD processID) {
	TCHAR szProcessName[MAX_PATH] = TEXT("<unknown>");
	// Get a handle to the process.
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION |
		PROCESS_VM_READ,
		FALSE, processID);
	// Get the process name.
	if (NULL != hProcess) {
		HMODULE hMod;
		DWORD cbNeeded;
		if (EnumProcessModules(hProcess, &hMod, sizeof(hMod),
			&cbNeeded)) {
			GetModuleBaseName(hProcess, hMod, szProcessName,
				sizeof(szProcessName) / sizeof(TCHAR));
		}
		// Release the handle to the process.
	}
	// Print the process name and identifier.
	_tprintf(_T("Process Name: %s\n"), szProcessName);
	CloseHandle(hProcess);
}
