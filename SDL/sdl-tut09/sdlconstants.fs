\ ===[ Code Addendum 05 ]============================================
\                 gforth: SDL/OpenGL Graphics Part IX
\ ===================================================================
\    File Name: sdlconstants.fs
\       Author: Timothy Trussell
\         Date: 02/26/2010
\  Description: Define SDL constants for sdl-vboll demo
\ Forth System: gforth-0.7.0
\ Linux System: Ubuntu v9.10 i386, kernel 2.6.31-19
\ C++ Compiler: gcc (Ubuntu 4.4.1-4ubuntu9) 4.4.1
\ ===================================================================
\ Added SDL_events.h enum tables for event types and masks 03/20/2010

\ SDL Version coding - do not really need these for gforth

1	constant SDL_MAJOR_VERSION
2	constant SDL_MINOR_VERSION
14	constant SDL_PATCHLEVEL         \ may not be correct

\ Initialization Flags for sdl-init

$00000001	constant SDL_INIT_TIMER
$00000010	constant SDL_INIT_AUDIO
$00000020	constant SDL_INIT_VIDEO
$00000100	constant SDL_INIT_CDROM
$00000200	constant SDL_INIT_JOYSTICK
$00100000	constant SDL_INIT_NOPARACHUTE
$01000000	constant SDL_INIT_EVENTTHREAD
$0000FFFF	constant SDL_INIT_EVERYTHING

$00000000	constant SDL_SWSURFACE
$00000001	constant SDL_HWSURFACE
$00000004	constant SDL_ASYNCBLIT
$10000000	constant SDL_ANYFORMAT
$20000000	constant SDL_HWPALETTE
$40000000	constant SDL_DOUBLEBUF
$80000000	constant SDL_FULLSCREEN
$00000002	constant SDL_OPENGL
$0000000	constant SDL_OPENGLBLIT
$00000010	constant SDL_RESIZABLE
$00000020	constant SDL_NOFRAME
$00000100	constant SDL_HWACCEL
$00001000	constant SDL_SRCCOLORKEY
$00002000	constant SDL_RLEACCELOK
$00004000	constant SDL_RLEACCEL
$00010000	constant SDL_SRCALPHA
$01000000	constant SDL_PREALLOC
$32315659	constant SDL_YV12_OVERLAY
$56555949	constant SDL_IYUV_OVERLAY
$32595559	constant SDL_YUY2_OVERLAY
$59565955	constant SDL_UYVY_OVERLAY
$55595659	constant SDL_YVYU_OVERLAY

\ Big/Little Endian specific
\ Set SDL_BYTEORDER for the Endian-ness of your computer.
\ My Athlon 64 X2 is Little Endian.

1234	constant SDL_BYTEORDER
1234	constant SDL_LIL_ENDIAN
4321	constant SDL_BIG_ENDIAN

0	constant NULL
1	constant SDL_VIDEO_OPENGL

\ ---[ SDL_Video ]---------------------------------------------------

1	constant SDL_VIDEO_DRIVER_DC
1	constant SDL_VIDEO_DRIVER_CYBERGRAPHICS
1	constant SDL_VIDEO_DRIVER_DUMMY
1	constant SDL_VIDEO_DRIVER_AALIB
1	constant SDL_VIDEO_DRIVER_CACA
1	constant SDL_VIDEO_DRIVER_DGA
1	constant SDL_VIDEO_DRIVER_DIRECTFB
1	constant SDL_VIDEO_DRIVER_FBCON
1	constant SDL_VIDEO_DRIVER_SVGALIB
1	constant SDL_VIDEO_DRIVER_X11
1	constant SDL_VIDEO_DRIVER_X11_DGAMOUSE
1	constant SDL_VIDEO_DRIVER_X11_DPMS
1	constant SDL_VIDEO_DRIVER_X11_VIDMODE
1	constant SDL_VIDEO_DRIVER_X11_XINERAMA
1	constant SDL_VIDEO_DRIVER_X11_XME
1	constant SDL_VIDEO_DRIVER_X11_XRANDR
1	constant SDL_VIDEO_DRIVER_X11_XV
1	constant SDL_VIDEO_DRIVER_DRAWSPROCKET
1	constant SDL_VIDEO_DRIVER_TOOLBOX
1	constant SDL_VIDEO_DRIVER_QUARTZ
1	constant SDL_VIDEO_OPENGL_GLX
1	constant SDL_VIDEO_DRIVER_OS2FS
1	constant SDL_VIDEO_DRIVER_GAPI
1	constant SDL_VIDEO_DRIVER_DDRAW
1	constant SDL_VIDEO_DRIVER_WINDIB
1	constant SDL_VIDEO_OPENGL_WGL

255	constant SDL_ALPHA_OPAQUE
0	constant SDL_ALPHA_TRANSPARENT
$01	constant SDL_LOGPAL
$02	constant SDL_PHYSPAL

\ ---[ SDL_Audio ]---------------------------------------------------

$0008	constant AUDIO_U8
$8008	constant AUDIO_S8
$0010	constant AUDIO_U16LSB
$8010	constant AUDIO_S16LSB
$1010	constant AUDIO_U16MSB
$9010	constant AUDIO_S16MSB
128	constant SDL_MIX_MAXVOLUME
$00	constant SDL_AUDIO_TRACK
1	constant SDL_AUDIO_DRIVER_DC
1	constant SDL_AUDIO_DRIVER_AHI
1	constant SDL_AUDIO_DRIVER_DISK
1	constant SDL_AUDIO_DRIVER_DUMMY
1	constant SDL_AUDIO_DRIVER_ALSA
1	constant SDL_AUDIO_DRIVER_ARTS
1	constant SDL_AUDIO_DRIVER_ESD
1	constant SDL_AUDIO_DRIVER_NAS
1	constant SDL_AUDIO_DRIVER_OSS
1	constant SDL_AUDIO_DRIVER_SNDMGR
1	constant SDL_AUDIO_DRIVER_COREAUDIO
1	constant SDL_AUDIO_DRIVER_DSOUND
1	constant SDL_AUDIO_DRIVER_WAVEOUT
1	constant SDL_AUDIO_DRIVER_DART

\ ---[ SDL_Timer ]---------------------------------------------------

1	constant SDL_TIMER_AMIGA
1	constant SDL_TIMER_DC
1	constant SDL_TIMER_UNIX
1	constant SDL_TIMER_MACOS
1	constant SDL_TIMERS_DISABLED
1	constant SDL_TIMER_OS2
1	constant SDL_TIMER_WINCE
1	constant SDL_TIMER_WIN32
10	constant SDL_TIMESLICE
10	constant TIMER_RESOLUTION


\ ---[ SDL_Joystick and Mouse ]--------------------------------------

1	constant SDL_JOYSTICK_DUMMY
1	constant SDL_JOYSTICK_MACOS
1	constant SDL_JOYSTICK_IOKIT
1	constant SDL_JOYSTICK_DISABLED
1	constant SDL_JOYSTICK_WINMM
1	constant SDL_JOYSTICK_OS2
1	constant SDL_JOYSTICK_AMIGA
1	constant SDL_JOYSTICK_DC
1	constant SDL_JOYSTICK_LINUX
$00	constant SDL_HAT_CENTERED
$01	constant SDL_HAT_UP
$02	constant SDL_HAT_RIGHT
$04	constant SDL_HAT_DOWN
$08	constant SDL_HAT_LEFT
-1	constant SDL_ALL_HOTKEYS
500	constant SDL_DEFAULT_REPEAT_DELAY
30	constant SDL_DEFAULT_REPEAT_INTERVAL
1	constant SDL_BUTTON_LEFT
2	constant SDL_BUTTON_MIDDLE
3	constant SDL_BUTTON_RIGHT
4	constant SDL_BUTTON_WHEELUP
5	constant SDL_BUTTON_WHEELDOWN

\ ---[ OS types ]----------------------------------------------------

1	constant __AIX__
1	constant __AMIGA__
1	constant __BEOS__
1	constant __BSDI__
1	constant __DREAMCAST__
1	constant __FREEBSD__
1	constant __HPUX__
1	constant __IRIX__
1	constant __LINUX__
1	constant __MACOSX__
1	constant __MACOS__
1	constant __NETBSD__
1	constant __OPENBSD__
1	constant __OS2__
1	constant __OSF__
1	constant __QNXNTO__
1	constant __RISCOS__
1	constant __SOLARIS__
1	constant __WIN32__
1	constant WIN32_LEAN_AND_MEAN

\ ---[ CDROM ]-------------------------------------------------------

1	constant SDL_CDROM_DUMMY
1	constant SDL_CDROM_DC
1	constant SDL_CDROM_LINUX
1	constant SDL_CDROM_MACOS
1	constant SDL_CDROM_MACOSX
1	constant SDL_CDROM_DISABLED
1	constant SDL_CDROM_OS2
1	constant SDL_CDROM_WIN32
75	constant CD_FPS

\ ---[ Threads ]-----------------------------------------------------

1	constant SDL_THREAD_AMIGA
1	constant SDL_THREAD_DC
1	constant SDL_THREAD_PTHREAD
1	constant SDL_THREAD_PTHREAD_RECURSIVE_MUTEX
1	constant SDL_THREADS_DISABLED
1	constant SDL_THREAD_OS2
1	constant SDL_THREAD_WIN32

\ ---[ LoadSO ]------------------------------------------------------

1	constant SDL_LOADSO_DUMMY
1	constant SDL_LOADSO_DLOPEN
1	constant SDL_LOADSO_DISABLED
1	constant SDL_LOADSO_MACOS
1	constant SDL_LOADSO_DLCOMPAT
1	constant SDL_LOADSO_OS2
1	constant SDL_LOADSO_WIN32

\ Everything else I have not looked into

$01	constant SDL_APPMOUSEFOCUS
$02	constant SDL_APPINPUTFOCUS
$04	constant SDL_APPACTIVE

99	constant SDL_MAX_TRACKS
$04	constant SDL_DATA_TRACK

1	constant SDL_HAS_64BIT_TYPE
1	constant STDC_HEADERS

1	constant SDL_ASSEMBLY_ROUTINES
1	constant SDL_HERMES_BLITTERS
1	constant SDL_ALTIVEC_BLITTERS

0	constant SDL_IGNORE
0	constant SDL_DISABLE
1	constant SDL_ENABLE

0	constant SDL_RELEASED
1	constant SDL_PRESSED

1	constant SDL_MUTEX_TIMEDOUT
1	constant NeedFunctionPrototypes
0	constant RW_SEEK_SET
1	constant RW_SEEK_CUR
2	constant RW_SEEK_END

13	constant SDL_GL_MULTISAMPLEBUFFERS
14	constant SDL_GL_MULTISAMPLESAMPLES

\ ---[ Event enumerations ]------------------------------------------

 0 constant SDL_NOEVENT                      \ Unused (do not remove)
 1 constant SDL_ACTIVEEVENT      \ Application loses/gains visibility
 2 constant SDL_KEYDOWN                                \ Keys pressed
 3 constant SDL_KEYUP                                 \ Keys released
 4 constant SDL_MOUSEMOTION                             \ Mouse moved
 5 constant SDL_MOUSEBUTTONDOWN                \ Mouse button pressed
 6 constant SDL_MOUSEBUTTONUP                 \ Mouse button released
 7 constant SDL_JOYAXISMOTION                  \ Joystick axis motion
 8 constant SDL_JOYBALLMOTION             \ Joystick trackball motion
 9 constant SDL_JOYHATMOTION           \ Joystick hat position change
10 constant SDL_JOYBUTTONDOWN               \ Joystick button pressed
11 constant SDL_JOYBUTTONUP                \ Joystick button released
12 constant SDL_QUIT                            \ User-requested quit
13 constant SDL_SYSWMEVENT                    \ System specific event
14 constant SDL_EVENT_RESERVEDA           \ Reserved for future use..
15 constant SDL_EVENT_RESERVEDB           \ Reserved for future use..
16 constant SDL_VIDEORESIZE                 \ User resized video mode
17 constant SDL_VIDEOEXPOSE              \ Screen needs to be redrawn
18 constant SDL_EVENT_RESERVED2           \ Reserved for future use..
19 constant SDL_EVENT_RESERVED3           \ Reserved for future use..
20 constant SDL_EVENT_RESERVED4           \ Reserved for future use..
21 constant SDL_EVENT_RESERVED5           \ Reserved for future use..
22 constant SDL_EVENT_RESERVED6           \ Reserved for future use..
23 constant SDL_EVENT_RESERVED7           \ Reserved for future use..

\ Events SDL_USEREVENT through SDL_MAXEVENTS-1 are for your use

24 constant SDL_USEREVENT

\ This last event is only for bounding internal arrays
\ It is the number of bits in the event mask datatype -- Uint32

32 constant SDL_NUMEVENTS

\ ---[ Predefined event masks ]--------------------------------------

: SDL-EventMask { _x -- 1<<_x } 1 _x LSHIFT ;

SDL_ACTIVEEVENT     SDL-EventMask    constant SDL_ACTIVEEVENTMASK
SDL_KEYDOWN         SDL-EventMask    constant SDL_KEYDOWNMASK
SDL_KEYUP           SDL-EventMask    constant SDL_KEYUPMASK

SDL_KEYDOWNMASK SDL_KEYUPMASK OR     constant SDL_KEYEVENTMASK

SDL_MOUSEMOTION     SDL-EventMask    constant SDL_MOUSEMOTIONMASK
SDL_MOUSEBUTTONDOWN SDL-EventMask    constant SDL_MOUSEBUTTONDOWNMASK
SDL_MOUSEBUTTONUP   SDL-EventMask    constant SDL_MOUSEBUTTONUPMASK

SDL_MOUSEMOTIONMASK
SDL_MOUSEBUTTONDOWNMASK OR
SDL_MOUSEBUTTONUPMASK OR             constant SDL_MOUSEEVENTMASK

SDL_JOYAXISMOTION   SDL-EventMask    constant SDL_JOYAXISMOTIONMASK
SDL_JOYBALLMOTION   SDL-EventMask    constant SDL_JOYBALLMOTIONMASK
SDL_JOYHATMOTION    SDL-EventMask    constant SDL_JOYHATMOTIONMASK
SDL_JOYBUTTONDOWN   SDL-EventMask    constant SDL_JOYBUTTONDOWNMASK
SDL_JOYBUTTONUP     SDL-EventMask    constant SDL_JOYBUTTONUPMASK

SDL_JOYAXISMOTIONMASK
SDL_JOYBALLMOTIONMASK OR
SDL_JOYHATMOTIONMASK OR
SDL_JOYBUTTONDOWNMASK OR
SDL_JOYBUTTONUPMASK OR               constant SDL_JOYEVENTMASK

SDL_VIDEORESIZE     SDL-EventMask    constant SDL_VIDEORESIZEMASK
SDL_VIDEOEXPOSE     SDL-EventMask    constant SDL_VIDEOEXPOSEMASK
SDL_QUIT            SDL-EventMask    constant SDL_QUITMASK
SDL_SYSWMEVENT      SDL-EventMask    constant SDL_SYSWMEVENTMASK

-1 constant SDL_ALLEVENTS


