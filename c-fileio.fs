\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2010, Simon Dobson <simon.dobson@computer.org>.
\ All rights reserved.
\
\ Attila is free software; you can redistribute it and/or
\ modify it under the terms of the GNU General Public License
\ as published by the Free Software Foundation; either version 2
\ of the License, or (at your option) any later version.
\
\ Attila is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program; if not, write to the Free Software
\ Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

\ File I/O primitives using C library functions
\
\ We use operating-system-level access primitives and do buffering
\ at the Attila level. All primitives are non-blocking.
\
\ This file has to export a primitive to reset the input system:
\
\   - reset_io       RESET-IO

CHEADER:
#include <unistd.h>
#include <stdio.h>
#include <fcntl.h>
#include <errno.h>
#include <sys/time.h>
#include <sys/select.h>

;CHEADER

  
\ ---------- File modes and flags ----------

C: R/W ( -- m )
   m = O_RDWR;
;C

C: R/O ( -- m )
  m = O_RDONLY;
;C

C: W/O ( -- m )
  m = O_WRONLY;
;C

\ No difference between text and binary files in Unix
C: BIN ( m -- m )
;C


\ ---------- File handling ----------

\ Delete a file, returning an error code (0 for success)
C: DELETE-FILE ( addr namelen -- ior )
  CHARACTERPTR fn;

  fn = create_unix_string(addr, namelen);
  ior = (CELL) unlink(fn);
;C

\ Open a file, returning a file handle and error code (0 for success)
C: OPEN-FILE ( addr namelen m -- fh ior )
  CHARACTERPTR fn;

  fn = create_unix_string(addr, namelen);
  fh = (CELL) open(fn, m);
  ior = (fh < 0) ? errno : 0;
;C

\ Create a file, returning a file handle and error code (0 for success).
C: CREATE-FILE ( addr namelen m -- fh ior )
  CHARACTERPTR fn;
  int mode;

  fn = create_unix_string(addr, namelen);
  fh = (CELL) open(fn, m | O_CREAT | O_TRUNC, 0655);
  ior = (fh == -1) ? (CELL) errno : 0;
;C

\ Close a file
C: CLOSE-FILE ( fh -- ior )
  ior = (CELL) close(fh);
;C

\ Set the positon of the next read/write action
C: REPOSITION-FILE ( off fh -- ior )      
  int m;

  if(fh != STDIN_FILENO) {
    m = lseek(fh, off, SEEK_SET);
    ior = (m == -1) ? (CELL) errno : 0;
  } else {
    ior = 0;
  }
;C

\ Return the current read/write offset
C: FILE-POSITION ( fh -- off ior ) 
  if(fh != STDIN_FILENO) {
    off = lseek(fh, 0, SEEK_CUR);
    ior = (off == -1) ? (CELL) errno : 0;      
  } else {
    off = 0;
    ior = 0;
  }
;C

\ Return the current input source: the user (0), a string (-1)
\ or a file (a FILE *)
C: SOURCE-ID ( -- id )
  int f;

  f = (int) (*user_variable(USER_INPUTSOURCE)); 
  if(f == -1)
    id = -1;
  else if(f == STDIN_FILENO)
    id = 0;
  else
    id = (CELL) f;
;C

  
\ ---------- Low-level non-blocking I/O ----------

\ Check whether a given file descriptor can be read, returning a
\ success flag and an error code. This is a non-blocking poll,
\ with the success flag TRUE only if there is data
C: FILE-READ-READY? read_ready ( fh -- f ior )      
  int fd;
  int nfds;
  fd_set fds;
  struct timeval interval;
  
  FD_ZERO(&fds);
  FD_SET(fh, &fds);
  nfds = fh + 1;
  interval.tv_sec = 0;   interval.tv_usec = 50; // sd: should be variable?
  ior = select(nfds, &fds, NULL, &fds, &interval);
  f = (ior > 0) ? TRUE : FALSE;
  ior = (ior < 0) ? errno : 0;    
;C
    
\ Read characters from the given file into a buffer.
\ This routine will return ( -- 0 0 ) if no characters have been
\ read but the call was successful, and ( -- -1 0) if the
\ end of file was reached
C: (READ-FILE) prim_read_file ( addr n fh -- m ior )
  CELL f;
      
  // check for readable characters
  m = 0;
  PUSH_CELL(fh);
  CALL(read_ready);
  ior = POP_CELL();   f = POP_CELL();
  if((ior == 0) && f) {
    m = read(fh, (CHARACTERPTR) addr, n);
    ior = (m < 0) ? (CELL) errno : 0;
    if((ior == 0) && (m == 0))
      m = -1; // EOF marker  
  }
;C

\ Write characters to the given file from the buffer
C: (WRITE-FILE) prim_write_file ( addr n fh -- ior )
  int m;
      
  m = write(fh, (CHARACTERPTR) addr, n);
  ior = (m < 0) ? (CELL) errno : 0;
;C


\ ---------- Character-based I/O ----------

\ Emit a character on the given file
C: (EMIT) ( c fh -- ior )
  CHARACTER ch;

  ch = (CHARACTER) c;
  PUSH_CELL((CELL) &ch);
  PUSH_CELL(sizeof(CHARACTER));
  PUSH_CELL(fh);
  CALL(prim_write_file);
  ior = POP_CELL();
;C

      
\ ---------- Initialising and resetting the I/O sub-system ----------
\ sd: typically called from hooks, hence the 0 return code      

\ Initialise to default streams
C: INIT-I/O init_io ( -- )
  *user_variable(USER_INPUTSOURCE) = STDIN_FILENO;
  *user_variable(USER_OUTPUTSINK) = STDOUT_FILENO;   
  PUSH_CELL(0);
;C

\ Reset to default streams
C: RESET-I/O reset_io ( -- )
  if(*user_variable(USER_INPUTSOURCE) != STDIN_FILENO)
    close(*user_variable(USER_INPUTSOURCE));
  *user_variable(USER_INPUTSOURCE) = STDIN_FILENO;
  if(*user_variable(USER_OUTPUTSINK) != STDOUT_FILENO)
    close(*user_variable(USER_OUTPUTSINK));
  *user_variable(USER_OUTPUTSINK) = STDOUT_FILENO; 
  PUSH_CELL(0);
;C

