// DpiInfoTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
using namespace std;

// Function prototypes
BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam);
PROCESS_DPI_AWARENESS GetDpiAwareness(DWORD processID);
void GetProcessNameAndID(DWORD processID, LPTSTR szProcessName, int len);
void PrintProcessNameAndID(DWORD processID);

int main() {
	_tprintf(_T("Starting DpiInfoTest\n\n"));

	EnumWindows(EnumWindowsProc, NULL);

	//HWND hWnd = GetActiveWindow();
	//DPI_AWARENESS_CONTEXT context = GetWindowDpiAwarenessContext(hWnd);
	//cout << "hWnd=" << hWnd << endl;
	//cout << "context=" << context << endl;

	return 0;
}


BOOL CALLBACK EnumWindowsProc(HWND hWnd, LPARAM lParam) {
	TCHAR processName[MAX_PATH] = _T("");
	TCHAR className[512];
	TCHAR title[512];
	int bytesClassName = GetClassName(hWnd, className, sizeof(className) / sizeof(TCHAR));
	int bytesWindowText = GetWindowText(hWnd, title, sizeof(title) / sizeof(TCHAR));
	int dpi = GetDpiForWindow(hWnd);
	DWORD processID = 0;
	GetWindowThreadProcessId(hWnd, &processID);
	GetProcessNameAndID(processID, processName, sizeof(processName) / sizeof(TCHAR));
	int bytesProcessName = _tcslen(processName);
	// Only return the ones with process names
	if (bytesProcessName == 0) return true;

	PROCESS_DPI_AWARENESS dpiAwareness = GetDpiAwareness(processID);
	DPI_AWARENESS_CONTEXT context = GetWindowDpiAwarenessContext(hWnd);

	_tprintf(_T("Process Name: [%d] %s\n"), bytesProcessName, processName);
	_tprintf(_T("HWND: %p\n"), hWnd);
	_tprintf(_T("PID: %d (0x%08x)\n"), processID, processID);
	_tprintf(_T("Window Title: [%d] %s\n"), bytesWindowText, title);
	_tprintf(_T("DPI: %d\n"), dpi);
	switch (dpiAwareness) {
	case PROCESS_DPI_UNAWARE:
		_tprintf(_T("DPI Awareness: PROCESS_DPI_UNAWARE\n"));
		break;
	case DPI_AWARENESS_SYSTEM_AWARE:
		_tprintf(_T("DPI Awareness: DPI_AWARENESS_SYSTEM_AWARE\n"));
		break;
	case DPI_AWARENESS_PER_MONITOR_AWARE:
		_tprintf(_T("DPI Awareness: DPI_AWARENESS_PER_MONITOR_AWARE\n"));
		break;
	default:
		_tprintf(_T("DPI Awareness: \n"));
		break;
	}
	_tprintf(_T("DPI_AWARENESS_CONTEXT: 0x%p (%d)\n"), context, (long)context);
	_tprintf(_T("\n"));

	return TRUE;
}

PROCESS_DPI_AWARENESS GetDpiAwareness(DWORD processID) {
	PROCESS_DPI_AWARENESS dpiAwareness = PROCESS_DPI_UNAWARE;
	if (!processID) return dpiAwareness;
	int dpiAwarenessErr = 0;
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
		FALSE, processID);
	if (hProcess != NULL) {
		SetLastError(0);
		int res = GetProcessDpiAwareness(hProcess, &dpiAwareness);
		dpiAwarenessErr = GetLastError();
		CloseHandle(hProcess);
	} else {
		dpiAwareness = PROCESS_DPI_UNAWARE;
	}
	return dpiAwareness;
}

void GetProcessNameAndID(DWORD processID, LPTSTR processName, int len) {
	if (!processID) return;
	// Get a handle to the process.
	HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ,
		FALSE, processID);
	// Get the process name.
	if (NULL != hProcess) {
		HMODULE hMod;
		DWORD cbNeeded;
		if (EnumProcessModules(hProcess, &hMod, sizeof(hMod),
			&cbNeeded)) {
			GetModuleBaseName(hProcess, hMod, processName, len);
		}
		// Release the handle to the process.
	}
	CloseHandle(hProcess);
}


void PrintProcessNameAndID(DWORD processID) {
	TCHAR processName[MAX_PATH] = TEXT("<unknown>");
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
			GetModuleBaseName(hProcess, hMod, processName,
				sizeof(processName) / sizeof(TCHAR));
		}
		// Release the handle to the process.
	}
	// Print the process name and identifier.
	_tprintf(_T("Process Name: %s\n"), processName);
	CloseHandle(hProcess);
}
