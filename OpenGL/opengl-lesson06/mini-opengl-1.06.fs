\ ===[ Code Addendum 03 ]============================================
\                 gforth: OpenGL Graphics Lesson 06
\ ===================================================================
\           File: mini-opengl-1.06.fs
\         Author: Timothy Trussell
\           Date: 07/24/2010
\    Description: OpenGL libcc interface for NeHe OpenGL Tutorial 06
\   Forth System: gforth-0.7.0
\      Assembler: Built-in FORTH assembler
\   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
\   C++ Compiler: gcc version 4.4.3 (Ubuntu 4.4.3-4ubuntu5)
\ ===================================================================
\ This libcc interface contains the cumulatively added OpenGL code
\ function calls that are required for use with the gforth version of
\ the NeHe OpenGL Tutorial Lessons.
\ ===================================================================

UseLibUtil [if]
  c-library mini_opengl_lib
[else]
  c-library mini_opengl_lib06
[then]

s" GL" add-lib
s" GLU" add-lib

\c #if defined(__APPLE__) && defined(__MACH__)
\c      #include <OpenGL/gl.h>
\c      #include <OpenGL/glu.h>
\c #else
\c      #include <GL/gl.h>
\c      #include <GL/glu.h>
\c #endif

\ Initial Entries for Lesson 01
c-function gl-clear            glClear                      n -- void
c-function gl-clear-color      glClearColor           r r r r -- void
c-function gl-clear-depth      glClearDepth                 r -- void
c-function gl-enable           glEnable                     n -- void
c-function gl-depth-func       glDepthFunc                  n -- void
c-function gl-hint             glHint                     n n -- void
c-function gl-load-identity    glLoadIdentity                 -- void
c-function gl-matrix-mode      glMatrixMode                 n -- void
c-function gl-shade-model      glShadeModel                 n -- void
c-function gl-viewport         glViewport             n n n n -- void
c-function glu-perspective     gluPerspective         r r r r -- void

\ Additions for Lesson 02
c-function gl-begin            glBegin                      n -- void
c-function gl-end              glEnd                          -- void
c-function gl-translate-f      glTranslatef             r r r -- void
c-function gl-vertex-3f        glVertex3f               r r r -- void

\ Additions for Lesson 03
c-function gl-color-3f         glColor3f                r r r -- void

\ Additions for Lesson 04
c-function gl-rotate-f         glRotatef              r r r r -- void

\ Additions for Lesson 05
\ -- none

\ Additions for Lesson 06
c-function gl-bind-texture     glBindTexture              n n -- void
c-function gl-gen-textures     glGenTextures              n a -- void
c-function gl-tex-coord-2f     glTexCoord2f               r r -- void
c-function gl-tex-image-2d     glTexImage2D n n n n n n n n a -- void
c-function gl-tex-parameter-i  glTexParameteri          n n n -- void

end-c-library

include glconstants.fs

