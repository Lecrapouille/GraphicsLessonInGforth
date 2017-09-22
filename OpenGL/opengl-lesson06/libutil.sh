#!/bin/bash
# ===[ Code Addendum 06 ]============================================
#                 gforth: OpenGL Graphics Lesson 06
# ===================================================================
#           File: libutil.sh
#       Revision: 1.01; Supercedes previous version
#         Author: Timothy Trussell
#           Date: 07/24/2010
#    Description: Library Utility for gforth OpenGL Tutorials
#   Linux System: Ubuntu v10.04 LTS i386, kernel 2.6.31-23
# ===================================================================
# --- --- --- --- --- --- --- --- --- --- --- ---  --- --- --- --- --
# This is a bash script - do *not* try to execute this from gforth.
# Execute this from a terminal window with <./libutil.sh> or with
# <bash libutil.sh>. Or copy to /bin to make it globally available.
# For more information, see the libutil.txt file.
# --- --- --- --- --- --- --- --- --- --- --- ---  --- --- --- --- --
# Added 07/18/2010:
#    -- Processing of the mini-sdl-1.xx.fs files, as had been done
#       for the mini-opengl-1.xx.fs files.
#    -- Processing of the mini-mixer-1.xx.fs files (for Lesson 21)
#    -- Changed the command line switches; added -kill to remove the
#       ~/.gforth/libcc-named and ~/.gforth/opengl-libs directories.
# --- --- --- --- --- --- --- --- --- --- --- ---  --- --- --- --- --
# ToDo:
#       Reduce the processing to a single basic function to where the
#       code checks for the libcc interface versions of all libcc
#       files in the PWD.
# --- --- --- --- --- --- --- --- --- --- --- ---  --- --- --- --- --

cmdarg=$1                           # save the command line parameter

# ---[ Extract Latest Lesson Number ]--------------------------------
# This accesses the PWD and searches for an <opengl-lessonxx> file,
# to extract the <xx> for use with the rest of this code.
# Aborts if unable to locate an <opengl-lessonxx> file.
# This is being done prior to allocating the variables, as the result
# is used to create several of the variable strings.

# Some of this code has been referred to as "bash black magic"

set -- *                          # create argument list of PWD files
filenums=$#                          # the number of files in the PWD
filenames=("$@")                        # copy file names to an array
filelimit=$#                                   # create and end check
((filelimit++))                     # and increment to the exit value
let hit=0                                # set >0 if a match is found
let mixer=0                      # flag for if mini-mixer* is present
lesson="00"                                    # which lesson this is
latestsdl="00"                              # latest mini-sdl-1.xx.fs
latestmixer="00"                          # latest mini-mixer-1.xx.fs

echo "Processing directory for: OpenGL libcc interface"

for (( i=0;i<$filelimit;i++)); do
  thisfile=${filenames[${i}]:0:13}
  if [ "${thisfile}" = "opengl-lesson" ]; then
    if [ $hit = "0" ]; then
      let hit=i                                  # set to first match
      lesson=${filenames[${i}]:13:2}          # set the lesson number
    else           # check to see if the next hit is a newer Tutorial
      if [ "$lesson" -lt "${filenames[${i}]:13:2}" ]; then
        lesson=${filenames[${i}]:13:2}   # set to newer lesson number
      fi
    fi
  fi
done

if [ $hit = "0" ]; then
  echo "No <opengl-lesson> file found in listing - aborting"
  exit 1
fi

# ---[ Extract Latest SDL Revision ]---------------------------------

let hit=0                                 # reset for next file check

echo "Processing directory for: SDL libcc interface"

for (( i=0;i<$filelimit;i++)); do
  thisfile=${filenames[${i}]:0:11}
  if [ "${thisfile}" = "mini-sdl-1." ]; then
    if [ $hit = "0" ]; then
      let hit=i                                  # set to first match
      latestsdl=${filenames[${i}]:11:2}       # set the lesson number
    else           # check to see if the next hit is a newer Tutorial
      if [ "$latestsdl" -lt "${filenames[${i}]:11:2}" ]; then
        latestsdl=${filenames[${i}]:11:2}             # set to latest
      fi
    fi
  fi
done

if [ $hit = "0" ]; then
  echo "No <mini-sdl-1.xx.fs> file found in listing - aborting"
  exit 1
fi

# ---[ Extract Latest Mixer Revision ]-------------------------------
# This does not abort out, as it is not required to be present for
# the majority of the Tutorial Lessons, unlike the previous two.

let hit=0                                 # reset for next file check

echo "Processing directory for: SDL_Mixer libcc interface"

for (( i=0;i<$filelimit;i++)); do
  thisfile=${filenames[${i}]:0:13}
  if [ "${thisfile}" = "mini-mixer-1." ]; then
    if [ $hit = "0" ]; then
      let hit=i                                  # set to first match
      let mixer=1                         # set mixer present to TRUE
      latestmixer=${filenames[${i}]:13:2}     # set the lesson number
    else           # check to see if the next hit is a newer Tutorial
      if [ "$latestmixer" -lt "${filenames[${i}]:13:2}" ]; then
        latestmixer=${filenames[${i}]:13:2}           # set to latest
      fi
    fi
  fi
done

if [ $hit = "0" ]; then
  echo "No <mini-mixer-1.xx.fs> file found in listing"
fi

# ---[ Define strings ]----------------------------------------------

version="1.01"                               # libutil version number
dest="${HOME}/.gforth/opengl-libs"            # destination directory
ogldst="mini-opengl-current.fs"         # OpenGL libcc dest file name
sdldst="mini-sdl-current.fs"               # SDL libcc dest file name
mixdst="mini-mixer-current.fs"       # SDL_Mixer libcc dest file name

# Listing of files included in the distribution

file[1]="opengl-lesson${lesson}-info.txt"           # descriptor file
file[2]="opengl-lesson${lesson}.txt"                    # lesson text
file[3]="opengl-lesson${lesson}.fs"                       # base code
file[4]="opengllib-1.${lesson}.fs"                      # lesson code
file[5]="mini-opengl-1.${lesson}.fs"         # OpenGL libcc interface
file[6]="mini-sdl-1.${latestsdl}.fs"            # SDL libcc interface
file[7]="glconstants.fs"                           # OpenGL constants
file[8]="sdlconstants.fs"                             # SDL constants
file[9]="sdlkeysym.fs"                       # SDL keyboard constants
file[10]="mini-mixer-1.${latestmixer}.fs" # SDL_Mixer libcc interface
file[11]="libutil.txt"              # install script file description
file[12]="libutil.sh"                           # install script file

# PWD does NOT access your password.
# It returns the full path to the current working directory.

oglsource="${PWD}/${file[5]}"            # expanded opengl source dir
ogltarget="${dest}/${ogldst}"       # expanded opengl destination dir
sdlsource="${PWD}/${file[6]}"               # expanded sdl source dir
sdltarget="${dest}/${sdldst}"          # expanded sdl destination dir
mixsource="${PWD}/${file[10]}"            # expanded mixer source dir
mixtarget="${dest}/${mixdst}"        # expanded mixer destination dir

let ferror=0                               # a flag for missing files

# ---[ Functions ]---------------------------------------------------
# Functions are declared before they can be called - like in Forth

function display_title() {
  clear                                     # clears terminal display
  echo -e "\t\t\t -----------------------------" # -e does formatting
  echo -e "\t\t\t gforth OpenGL Library Utility"
  echo -e "\t\t\t         Version ${version}"
  echo -e "\t\t\t -----------------------------"
  echo
}

function display_usage() {
  echo
  echo "LibUtil Usage:"
  echo
  echo "   libutil.sh -i    installs lesson files"
  echo "   libutil.sh -l    displays ~/.gforth/.. directories"
  echo "   libutil.sh -ra   deletes all mini_*_lib files"
  echo "   libutil.sh -ro   deletes compiled mini_opengl_lib files"
  echo "   libutil.sh -rm   deletes compiled mini_mixer_lib files"
  echo "   libutil.sh -rs   deletes compiled mini_sdl_lib files"
  echo "   libutil.sh -kill deletes the following directories:"
  echo "                      <~/.gforth/libcc-named>"
  echo "                      <~/.gforth/opengl-libs>"
  echo "   libutil.sh -?    shows this help"
  echo
}

function abort_message() {
  echo -e "\t\t\t     Aborting installation"
  echo
  echo "-- Could not locate the hidden directory <.gforth>."
  echo "-- gforth must be installed prior to using this script."
  echo "-- <.gforth> should be in the ${HOME} directory."
  echo
}

function verify_source_files() {
  for i in {5..9}
    do
      if ! [ -f ${PWD}/${file[i]} ]; then
        # File not found - display file and increment error counter
        echo "Error - missing file: <${file[i]}>"
        ((ferror++))
      fi    
    done
  # If ferror>0 then we are missing files, so abort
  if [ $ferror -gt "0" ]; then
    echo
    exit 1
  fi
}

# Ask if addresses are correct; exit the program if not
function verify_directories() {
  echo "File Source Directory:"
  echo "${PWD}"
  echo
  echo "File Destination Directory:"
  echo "${dest}"
  echo
  if [ -d ${dest} ]; then
    echo "(Target directory exists)"
  else
    echo "(Target directory does not exist, and will be created)"
  fi
  echo

  read -p "Do you wish to proceed (y/n)?"

  # if anything but <y> is pressed exit
  if [ -z $REPLY ] || [ $REPLY != "y" ]; then
    echo
    exit 0
  fi
}

# Check to see if the target directory exists; if not, create it
function create_target_directory() {
  if ! [ -d $dest ]; then
    mkdir $dest
    echo "Directory ${dest} created"
  fi
}

# Check to see if the target file exists; delete if it does
function delete_old_opengl() {  
  if [ -f $ogltarget ]; then
    echo "Deleting old ~/.gforth/opengl-libs/${ogldst}"
    rm ${ogltarget}
  fi
}

# Copy mini-opengl-1.xx.fs to mini-opengl-current.fs
function copy_new_opengl() {
  echo "Copying ${file[5]} to ~/.gforth/opengl-libs/${ogldst}"
  cp ${oglsource} ${ogltarget}
}

# Check to see if the target file exists; delete if it does
function delete_old_sdl() {  
  if [ -f $sdltarget ]; then
    echo "Deleting old ~/.gforth/opengl-libs/${sdldst}"
    rm ${sdltarget}
  fi
}

# Copy mini-sdl-1.xx.fs to mini-sdl-current.fs
function copy_new_sdl() {
  echo "Copying ${file[6]} to ~/.gforth/opengl-libs/${sdldst}"
  cp ${sdlsource} ${sdltarget}
}

# Check to see if the target file exists; delete if it does
function delete_old_mixer() {  
  if [ -f $mixtarget ]; then
    echo "Deleting old ~/.gforth/opengl-libs/${mixdst}"
    rm ${mixtarget}
  fi
}

# Copy mini-sdl-1.xx.fs to mini-sdl-current.fs if it exists
function copy_new_mixer() {
  if [ $mixer = "1" ]; then
    echo "Copying ${file[10]} to ~/.gforth/opengl-libs/${mixdst}"
    cp ${mixsource} ${mixtarget}
  fi
}

# If an SDL file is not in the destination directory, copy it.
# verify_source_files already checked to see if the source files are
# available to be copied.
function check_sdl_dependancies() {
  echo "Checking gforth/SDL dependancy files"
  for i in {7..9}
    do
      if ! [ -f ${dest}/${file[i]} ]; then
        # file is not present, so try to copy it
        cp ${PWD}/${file[i]} ${dest}/${file[i]}
        echo "-- ${file[i]} copied to ${dest}"
      fi
    done
  if [ mixer = 1 ]; then
    # if mini-mixer* is present, try to copy it also
    cp ${PWD}/${file[10]} ${dest}/${file[10]}
    echo "-- ${file[10]} copied to ${dest}"
  fi
}

# Removes compiled <mini_opengl_lib> files if they are present
function remove_mini_opengl_lib_files() {
  echo "Checking for previous mini_opengl_lib files"
  if [ -f ~/.gforth/libcc-named/mini_opengl_lib.c ]; then
    echo "Removing previous mini_opengl_lib compiled files:"
    echo "-- Removing ~/.gforth/libcc-named/mini_opengl_lib.*"
    rm ~/.gforth/libcc-named/mini_opengl_lib*
  fi

  if [ -f ~/.gforth/libcc-named/.libs/mini_opengl_lib.a ]; then
    echo "-- Removing ~/.gforth/libcc-named/.libs/mini_opengl_lib.*"
    rm ~/.gforth/libcc-named/.libs/mini_opengl_lib*
  fi
}

# Removes compiled <mini_sdl_lib> files if they are present
function remove_mini_sdl_lib_files() {
  echo "Checking for previous mini_sdl_lib files"
  if [ -f ~/.gforth/libcc-named/mini_sdl_lib.c ]; then
    echo "Removing previous mini_sdl_lib compiled files:"
    echo "-- Removing ~/.gforth/libcc-named/mini_sdl_lib.*"
    rm ~/.gforth/libcc-named/mini_sdl_lib*
  fi

  if [ -f ~/.gforth/libcc-named/.libs/mini_sdl_lib.a ]; then
    echo "-- Removing ~/.gforth/libcc-named/.libs/mini_sdl_lib.*"
    rm ~/.gforth/libcc-named/.libs/mini_sdl_lib*
  fi
}

# Removes compiled <mini_mixer_lib> files if they are present
function remove_mini_mixer_lib_files() {
  echo "Checking for previous mini_mixer_lib files"
  if [ -f ~/.gforth/libcc-named/mini_mixer_lib.c ]; then
    echo "Removing previous mini_mixer_lib compiled files:"
    echo "-- Removing ~/.gforth/libcc-named/mini_mixer_lib.*"
    rm ~/.gforth/libcc-named/mini_mixer_lib*
  fi

  if [ -f ~/.gforth/libcc-named/.libs/mini_mixer_lib.a ]; then
    echo "-- Removing ~/.gforth/libcc-named/.libs/mini_mixer_lib.*"
    rm ~/.gforth/libcc-named/.libs/mini_mixer_lib*
  fi
}

# Parse what the user entered on the command line
function parse_command_line() {
  # If <cr> or <-?> are entered display help data
  if [ -z ${cmdarg} ] || [ $cmdarg = "-?" ]; then
    display_usage
    # Exit after displaying the usage info
    exit 0
  fi

  # If <-ra> is passed, try to delete all mini_*_lib files
  if [ $cmdarg = "-ra" ]; then
    remove_mini_opengl_lib_files
    remove_mini_sdl_lib_files
    remove_mini_mixer_lib_files
    echo
    exit 0
  fi

  # If <-rm> is passed, try to delete mini_mixer_lib files
  if [ $cmdarg = "-rm" ]; then
    remove_mini_mixer_lib_files
    echo
    exit 0
  fi

  # If <-ro> is passed, try to delete mini_opengl_lib files
  if [ $cmdarg = "-ro" ]; then
    remove_mini_opengl_lib_files
    echo
    exit 0
  fi

  # If <-rs> is passed, try to delete mini_sdl_lib files
  if [ $cmdarg = "-rs" ]; then
    remove_mini_sdl_lib_files
    echo
    exit 0
  fi

  # If <-kill> is passed, delete /libcc-named and /opengl-libs dirs
  if [ $cmdarg = "-kill" ]; then
    if [ -d ~/.gforth/libcc-named ]; then
      echo "Removing directory: ~/.gforth/libcc-named"
      rm -r ~/.gforth/libcc-named
    fi
    if [ -d ~/.gforth/opengl-libs ]; then
      echo "Removing directory: ~/.gforth/opengl-libs"
      rm -r ~/.gforth/opengl-libs
    fi
    echo
    exit 0
  fi

  # If <-l> is passed, display directories - if they exist
  if [ $cmdarg = "-l" ]; then
    # if ~/.gforth/libcc-named exists, display the contents
    if [ -d ~/.gforth/libcc-named ]; then
      echo "Directory listing of <~/.gforth/libcc-named>:"
      echo
      dir ~/.gforth/libcc-named
      echo
    fi
    # if ~/.gforth/libcc-named/.libs exists, display the contents
    if [ -d ~/.gforth/libcc-named/.libs ]; then
      echo "Directory listing of <~/.gforth/libcc-named/.libs>:"
      echo
      dir ~/.gforth/libcc-named/.libs
      echo
    fi
    # if ~/.gforth/opengl-libs exists, display the contents
    if [ -d ~/.gforth/opengl-libs ]; then
      echo "Directory listing of <~/.gforth/opengl-libs>:"
      echo
      dir ~/.gforth/opengl-libs
      echo
    fi
    exit 0
  fi
  
  # If <-i> is passed continue, otherwise exit
  if ! [ $cmdarg = "-i" ]; then
    echo "Unrecognized command entered - exiting"
    display_usage
    exit 0
  fi
}

# ===[ Main Program ]================================================

# Say hello
display_title

# Verify presence of the /home/<user>/.gforth directory.
# If present, gforth has been installed, else abort.
# If it is not present, the rest of the program has nothing to do.

if ! [ -d ~/.gforth ]; then
  abort_message
  exit 1
fi

# Parse for specific command line options; returns if <-i> was passed
parse_command_line

# display current working directory 
echo "Current Directory: ${PWD}"
echo

# These function names should be descriptive enough
verify_source_files  
verify_directories
create_target_directory
delete_old_opengl
copy_new_opengl
delete_old_sdl
copy_new_sdl
delete_old_mixer
copy_new_mixer
check_sdl_dependancies
remove_mini_opengl_lib_files
remove_mini_sdl_lib_files
remove_mini_mixer_lib_files
  
echo "Installation completed."
echo

# ===================================[ Revision: 1.01 07/24/2010 ]===

