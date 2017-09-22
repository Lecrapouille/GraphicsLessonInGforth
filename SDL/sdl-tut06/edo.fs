\ ===[ Code Addendum 03 ]============================================
\             gforth: SDL/OpenGL Graphics Part VI
\ ===================================================================
\      Program: edo.fs
\       Author: G.T. Hawkins
\         Date: 07/28/1987
\         Mods: Timothy Trussell
\             : 05/05/2005
\             : 06/06/2006
\  Description: Tool for creating structures and arrays in FORTH
\ Forth System: All tested with
\    Assembler: Not used
\ ===================================================================

\ ===================================================================
\
\                   ______   _____    _______
\                  /\  ___\ /\  __ \ /\  ___ \
\                  \ \ \____  \ \  \ \ \ \  \ \
\                   \ \  ___\  \ \  \ \ \ \  \ \
\                    \ \ \__/_  \ \__/ \ \ \__\ \
\                     \ \______\/\_____/\ \______\
\                      \/______/\/____/  \/______/
\
\                                EDO
\
\                       Extended Data Objects
\ ===================================================================
\
\ Author:
\
\       G.T. Hawkins
\       I believe this may have also been printed in FORTH Dimensions
\
\ Revisions:
\
\       1.0     07/28/1987
\       1.1     05/05/2005    Change definition of 1WORD for use with
\                             a 32-bit FORTH system. (OS2FORTH)
\       1.2     06/06/2006    Added FLOAT* and 1FLOAT for use with a
\                             32-bit FORTH system (OS2FORTH)
\
\               06/06/2006    Changed definition layout of how I am
\                             suggesting to use the DEF and array
\                             definition formats.
\
\ Dates:
\
\       07/28/1987      Original Code
\       05/05/2005      updated to use with 32-bit WORDs
\       06/06/2006      added Floating Point usage
\
\ FORTH Version:
\
\       Works with all versions of FORTH I have used to date
\       ( LOVE4TH, FPC, GFORTH, WIN32FORTH, OS2FORTH )
\
\ Purpose:
\
\       To provide tools to create and work with data objects.
\
\       This could be considered "The Poor Man's" word set.
\       It is really a bare-bones implementation and does not supply
\       the complexity of the Pascal RECORD or the C/C++ STRUC
\       constructs.
\
\ Possible Additions:
\
\       >> DWORD, QWORD, TWORD, FWORD types
\          (double, quad, tera and float types, that is)
\       >> Bit-level access
\       >> Some level of error checking for out-of-bounds
\         (could be done in the index word code)
\
\ ===================================================================
\                            Definitions
\ ===================================================================

: [EDO] ;

1 CONSTANT 1BYTE                  \ standard size of a byte - 8 bits

cell CONSTANT 1WORD ( -- n )      \ the machine word/cell size

8 constant 1FLOAT                 \ for the 32Forth system only
                                  \ - - - at the moment

: BYTE* ;                         \ for clarity when defining a large
                                  \ number of bytes

: WORD+ ( n -- n+WORD )           \ increments by word size
  1WORD + ;

: WORD* ( n -- n*WORD )           \ multiplies by word size
  1WORD * ;

\ ---[ Note ]------------------------------------- 06/06/2006 tgt ---
\ FLOAT+ is already defined in the 32Forth Floating Point definitions
\ as follows:
\
\          /-----------------------------------\
\          |  : FLOATS ( n1 -- n2 )            |
\          |     8 * ;                         |
\          |                                   |
\          |  : FLOAT+ ( f-addr1 -- f-addr2 )  |
\          |     1 FLOATS + ;                  |
\          \-----------------------------------/
\
\ Again, for clarity, in the 32Forth system (OS2FORTH), the size of
\ a Floating Point number is 8 bytes, whether on the stack, or in a
\ FP variable (FVARIABLE), or as a FP constant (FCONSTANT) in the
\ dictionary.
\
\ Reference the dox in the \TOOLS\FLOATS.4th file for more info.
\ ----------------------------------------------------[ End Note ]---

: FLOAT* ( n -- n*FLOAT )
  FLOATS ;

: DEF   ( size -- )               \ defines a Forth scalar
  CONSTANT ;

\ ===================================================================
\ === The structure data object =====================================
\ ===================================================================

\ === Initiates a structure definition ===

: S{    ( -- 0 )
  0 ;

\ === Defines a structure component ===

: ::    ( offset object-definition -- offset )
  CREATE OVER , + DOES> @ + ;

\ === Ends a structure definition & defines a structure object ===

: }S    ( size -- )  \ definition name follows in input stream
  CONSTANT ;

\ ===================================================================
\ === The vector data object ========================================
\ ===================================================================

\ === Defines a vector and a vector operator ===

: []    ( object-definition #objects -- )
  OVER * CONSTANT                       \ define the vector
  CREATE ,                              \ define the vector operator
  DOES> @ * + ;


\ ---[Note]----------------------------------------------------------
\ For my own personal coding style, I have decided to utilize a set
\ of common definition words to define the basic byte and word sized
\ elements which I most commonly use.
\ -------------------------------------------------------[End Note]---

\ These are because I am using this file in 16- and 32-bit Forths

  1BYTE DEF {1BYTE}-DEF
2 BYTE* DEF {2BYTE}-DEF
4 BYTE* DEF {4BYTE}-DEF

\ This is the normal word definition - 2 byte in 16 bit, 4 in 32-bit
  1WORD DEF {1WORD}-DEF
2 WORD* DEF {2WORD}-DEF

\ This is specifically for 32Forth - appears to work with gforth also
8 BYTE* DEF {1FLOAT}-DEF

