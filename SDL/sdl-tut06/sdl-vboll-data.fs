\ ===[ Code Addendum 02 ]============================================
\             gforth: SDL/OpenGL Graphics Part VI
\ ===================================================================
\      Program: sdl-vboll-data.fs
\       Author: Timothy Trussell
\         Date: 02/28/2010
\  Description: Data for the SDL Vector Bolls Graphics Demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================

create SquareS[]
16 , 16 ,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C,

create DiamondS[]
16 , 16 ,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C,
1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C,
0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C,
0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C,
0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 1 C,
1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C,
1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C,
1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C,
0 C, 0 C, 1 C, 1 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C,
1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C,
1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C,
0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C,

create Ovoid[]
16 , 16 ,
0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 0 C, 0 C,
0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C,
0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 1 C, 1 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C, 0 C, 1 C,
1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 0 C,
0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
0 C, 0 C, 0 C, 0 C, 0 C, 0 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C, 1 C,
0 C, 0 C, 0 C, 0 C,

\ I had to add a field to make this conform to the SDL_Color struct:

\ typedef struct{
\   Uint8 r;
\   Uint8 g;
\   Uint8 b;
\   Uint8 unused;
\ } SDL_Color;

create VBPalette[]
\ 24*3 bytes
         0 C,  0 C,  0 C,  0 C, \ color # 0
         0 C,  0 C, 12 C,  0 C, \ color # 1
         4 C,  4 C, 12 C,  0 C, \ color # 2
         4 C,  4 C, 16 C,  0 C, \ color # 3
         4 C,  4 C, 20 C,  0 C, \ color # 4
         4 C,  4 C, 24 C,  0 C, \ color # 5
         8 C,  8 C, 24 C,  0 C, \ color # 6
         8 C,  8 C, 28 C,  0 C, \ color # 7
         8 C,  8 C, 32 C,  0 C, \ color # 8
         8 C,  8 C, 36 C,  0 C, \ color # 9
        12 C, 12 C, 40 C,  0 C, \ color # 10
        12 C, 12 C, 44 C,  0 C, \ color # 11
        12 C, 12 C, 48 C,  0 C, \ color # 12
        12 C, 12 C, 52 C,  0 C, \ color # 13
        16 C, 16 C, 48 C,  0 C, \ color # 14
        16 C, 16 C, 52 C,  0 C, \ color # 15
        16 C, 16 C, 56 C,  0 C, \ color # 16
        20 C, 20 C, 56 C,  0 C, \ color # 17
        20 C, 20 C, 60 C,  0 C, \ color # 18
        24 C, 24 C, 60 C,  0 C, \ color # 19
        28 C, 28 C, 60 C,  0 C, \ color # 20
        32 C, 32 C, 60 C,  0 C, \ color # 21
        36 C, 36 C, 60 C,  0 C, \ color # 22
        60 C, 60 C, 60 C,  0 C, \ color # 23
         0 C,  0 C, 47 C,  0 C, \ BLUE for anim
         0 C, 47 C,  0 C,  0 C, \ GREEN for anim
        47 C,  0 C,  0 C,  0 C, \ RED for anim

create VBBitMap[]
31 , 24 ,       \ height, width
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  8 C,  9 C, 
10 C, 10 C, 10 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C, 
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  8 C,  9 C,
10 C, 11 C, 11 C, 12 C, 12 C, 12 C, 11 C,  0 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  5 C,  8 C,
 9 C, 10 C, 11 C, 11 C, 12 C, 12 C, 13 C, 13 C, 12 C, 12 C, 10 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  5 C,
 7 C,  9 C, 10 C, 11 C, 11 C, 12 C, 12 C, 13 C, 15 C, 15 C, 15 C,
15 C, 12 C, 11 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,
 4 C,  7 C,  8 C,  9 C, 10 C, 11 C, 12 C, 12 C, 13 C, 15 C, 15 C,
16 C, 16 C, 15 C, 15 C, 12 C, 10 C,  0 C,  0 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  5 C,  7 C,  8 C,  9 C, 10 C, 11 C, 12 C, 12 C, 13 C,
15 C, 16 C, 16 C, 16 C, 16 C, 15 C, 13 C, 12 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  0 C,  4 C,  6 C,  8 C,  9 C, 10 C, 10 C, 11 C, 12 C, 
12 C, 13 C, 15 C, 16 C, 16 C, 16 C, 16 C, 16 C, 15 C, 12 C, 11 C, 
 0 C,  0 C,  0 C,  0 C,  2 C,  4 C,  7 C,  8 C,  9 C, 10 C, 10 C,
11 C, 12 C, 12 C, 13 C, 15 C, 15 C, 16 C, 16 C, 16 C, 16 C, 15 C, 
13 C, 12 C,  8 C,  0 C,  0 C,  0 C,  3 C,  5 C,  7 C,  8 C,  9 C, 
10 C, 10 C, 11 C, 12 C, 12 C, 13 C, 15 C, 15 C, 16 C, 16 C, 16 C, 
16 C, 15 C, 13 C, 12 C, 10 C,  0 C,  0 C,  0 C,  3 C,  5 C,  7 C, 
 8 C,  9 C,  9 C, 10 C, 11 C, 11 C, 12 C, 12 C, 15 C, 16 C, 16 C, 
16 C, 16 C, 15 C, 15 C, 13 C, 12 C, 11 C,  0 C,  0 C,  2 C,  4 C,  
 5 C,  7 C,  8 C,  9 C,  9 C, 10 C, 11 C, 11 C, 12 C, 12 C, 19 C, 
22 C, 18 C, 15 C, 15 C, 15 C, 15 C, 13 C, 12 C, 11 C,  8 C,  0 C,
 2 C,  4 C,  5 C,  7 C,  8 C,  8 C,  9 C, 10 C, 10 C, 11 C, 11 C, 
14 C, 22 C, 23 C, 22 C, 15 C, 15 C, 15 C, 13 C, 12 C, 12 C, 11 C, 
 9 C,  0 C,  2 C,  4 C,  5 C,  6 C,  7 C,  8 C,  9 C,  9 C, 10 C, 
11 C, 11 C, 12 C, 21 C, 23 C, 22 C, 15 C, 13 C, 13 C, 12 C, 12 C, 
12 C, 11 C,  9 C,  0 C,  2 C,  3 C,  4 C,  6 C,  7 C,  8 C,  9 C, 
 9 C, 10 C, 10 C, 11 C, 11 C, 15 C, 20 C, 17 C, 12 C, 12 C, 12 C, 
12 C, 12 C, 11 C, 10 C,  9 C,  0 C,  2 C,  3 C,  4 C,  5 C,  7 C, 
 8 C,  8 C,  9 C,  9 C, 10 C, 10 C, 11 C, 11 C, 12 C, 12 C, 12 C, 
12 C, 12 C, 12 C, 11 C, 11 C, 10 C,  9 C,  0 C,  2 C,  3 C,  4 C,  
 5 C,  7 C,  7 C,  8 C,  8 C,  9 C, 10 C, 10 C, 10 C, 11 C, 11 C, 
11 C, 11 C, 11 C, 11 C, 11 C, 11 C, 10 C, 10 C,  8 C,  0 C,  2 C,  
 3 C,  4 C,  4 C,  6 C,  7 C,  8 C,  8 C,  9 C,  9 C, 10 C, 10 C, 
10 C, 11 C, 11 C, 11 C, 11 C, 11 C, 11 C, 10 C, 10 C,  9 C,  8 C, 
 0 C,  2 C,  3 C,  3 C,  4 C,  5 C,  7 C,  7 C,  8 C,  8 C,  9 C, 
 9 C,  9 C, 10 C, 10 C, 10 C, 10 C, 10 C, 10 C, 10 C, 10 C,  9 C, 
 9 C,  7 C,  0 C,  2 C,  2 C,  3 C,  4 C,  5 C,  5 C,  7 C,  7 C,
 8 C,  8 C,  9 C,  9 C,  9 C, 10 C, 10 C, 10 C, 10 C, 10 C, 10 C, 
 9 C,  9 C,  8 C,  7 C,  0 C,  1 C,  2 C,  3 C,  4 C,  4 C,  5 C, 
 6 C,  7 C,  7 C,  8 C,  8 C,  9 C,  9 C,  9 C,  9 C,  9 C,  9 C, 
 9 C,  9 C,  9 C,  8 C,  7 C,  5 C,  0 C,  1 C,  2 C,  3 C,  3 C, 
 4 C,  4 C,  5 C,  6 C,  7 C,  7 C,  8 C,  8 C,  8 C,  8 C,  9 C, 
 9 C,  9 C,  9 C,  8 C,  8 C,  8 C,  7 C,  4 C,  0 C,  1 C,  2 C, 
 2 C,  3 C,  3 C,  4 C,  4 C,  5 C,  6 C,  7 C,  7 C,  7 C,  8 C, 
 8 C,  8 C,  8 C,  8 C,  8 C,  8 C,  7 C,  7 C,  5 C,  3 C,  0 C,
 0 C,  1 C,  2 C,  3 C,  3 C,  4 C,  4 C,  4 C,  5 C,  6 C,  7 C, 
 7 C,  7 C,  7 C,  7 C,  8 C,  8 C,  7 C,  7 C,  7 C,  6 C,  4 C, 
 0 C,  0 C,  0 C,  1 C,  2 C,  2 C,  3 C,  3 C,  4 C,  4 C,  4 C, 
 5 C,  5 C,  6 C,  7 C,  7 C,  7 C,  7 C,  7 C,  7 C,  6 C,  5 C, 
 4 C,  3 C,  0 C,  0 C,  0 C,  1 C,  1 C,  2 C,  2 C,  3 C,  3 C, 
 4 C,  4 C,  4 C,  5 C,  5 C,  5 C,  5 C,  6 C,  6 C,  6 C,  5 C, 
 5 C,  4 C,  4 C,  2 C,  0 C,  0 C,  0 C,  0 C,  1 C,  1 C,  2 C, 
 2 C,  3 C,  3 C,  3 C,  4 C,  4 C,  4 C,  4 C,  5 C,  5 C,  5 C,
 5 C,  4 C,  4 C,  4 C,  3 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,
 1 C,  1 C,  2 C,  2 C,  3 C,  3 C,  3 C,  3 C,  4 C,  4 C,  4 C,
 4 C,  4 C,  4 C,  4 C,  3 C,  3 C,  0 C,  0 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  1 C,  1 C,  1 C,  2 C,  2 C,  2 C,  3 C,  3 C,  3 C,
 3 C,  3 C,  3 C,  3 C,  3 C,  3 C,  3 C,  2 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  1 C,  1 C,  1 C,  2 C,  2 C,  2 C,
 2 C,  3 C,  3 C,  3 C,  3 C,  3 C,  3 C,  2 C,  2 C,  0 C,  0 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  1 C,  1 C,  1 C,
 1 C,  2 C,  2 C,  2 C,  2 C,  2 C,  2 C,  2 C,  2 C,  1 C,  0 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,
 0 C,  0 C,  1 C,  1 C,  1 C,  1 C,  1 C,  1 C,  1 C,  0 C,  0 C,
 0 C,  0 C,  0 C,  0 C,  0 C,  0 C,  0 C,

\ --[ Structures ]---------------------------------------------------

S{
  {1WORD}-DEF :: .delay
  {1WORD}-DEF :: .basedelay
  {1WORD}-DEF :: .&image
  {1WORD}-DEF :: .xcoord
  {1WORD}-DEF :: .xinc
  {1WORD}-DEF :: .xmin
  {1WORD}-DEF :: .xmax
  {1WORD}-DEF :: .ycoord
  {1WORD}-DEF :: .yinc
  {1WORD}-DEF :: .ymin
  {1WORD}-DEF :: .ymax
  {1WORD}-DEF :: .color
}S anim-def

anim-def 13 [] anim[]-def anim-ndx

\ ---[ Type Definitions ]--------------------------------------------
\ For completeness, I want to convert these to gforth structs

S{
  {1WORD}-DEF :: .height
  {1WORD}-DEF :: .width
  24 31 *     :: .image
}S sprite-def

S{
  {1WORD}-DEF :: .vbX
  {1WORD}-DEF :: .vbY
  {1WORD}-DEF :: .vbZ
  {1WORD}-DEF :: .vbC
}S vboll-def

{1FLOAT}-DEF 360 [] sc[]-def sc-ndx

create Sine[]   sc[]-def allot
create Cosine[] sc[]-def allot

vboll-def 8 [] vboll[]-def vboll-ndx

create vboll[]  here vboll[]-def dup allot 0 fill
create vboll2[] here vboll[]-def dup allot 0 fill
create temp[]   here vboll[]-def dup allot 0 fill

\ single dummy vector boll used for swapping

create dummy[]   here vboll-def dup allot 0 fill

