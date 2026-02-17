GCC=gcc.exe
WINDRES=windres.exe

all:
	mkdir -p ..\bin\$(_CONF)\netframework4.8\wmx
	$(WINDRES) --input wmx.rc --output wmx.res --output-format=coff
	$(GCC) -m64 -mwindows -DX64 -o ..\bin\$(_CONF)\netframework4.8\wmx\wmx64.exe wmx.c
	$(GCC) -m64 -DX64 -c wmxdll.c
	$(GCC) -shared -o ..\bin\$(_CONF)\netframework4.8\wmx\wmx64.dll wmxdll.o
	rm wmxdll.o
	rm wmx.res