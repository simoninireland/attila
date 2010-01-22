\ $Id$

\ File I/O primitives using host gcc and GNU readline
\
\ We use the C standard streams functions to manage files, since
\ they provide buffering etc. For some systems it might be necessary
\ to implement these directly.

CHEADER:
#include <unistd.h>
#include <stdio.h>
#include <fcntl.h>
#include <errno.h>

// File mode conversions
CHARACTERPTR
mode_string( CELL m ) {
  static CHARACTER mode[3];
  CHARACTERPTR s;

  mode[0] = mode[1] = mode[2] = '\0';   s = mode;

  if(m == O_RDONLY) {
    *s++ = 'r';
  } else if(m & O_WRONLY) {
    if(m & O_TRUNC) {
      *s++ = 'w';
    } else {
      *s++ = 'r';   *s++ = '+';
    }  
  } else if(m & O_RDWR) {
    *s++ = 'r';   *s++ = '+';
  }

  return mode;
}
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
  fh = (CELL) fopen(fn, mode_string(m));
  ior = (fh == 0) ? errno : 0;
;C

\ Create a file, returning a file handle and error code (0 for success).
C: CREATE-FILE ( addr namelen m -- fh ior )
  CHARACTERPTR fn;
  int mode;

  fn = create_unix_string(addr, namelen);
  fh = (CELL) fopen(fn, mode_string(m | O_TRUNC));
  ior = (fh == -1) ? (CELL) errno : 0;
;C

\ Close a file
C: CLOSE-FILE ( fh -- ior )
  ior = (CELL) fclose((FILE *) fh);
;C

\ Set the positon of the next read/write action
C: REPOSITION-FILE ( off fh -- ior )      
  int m;

  if(fh != stdin) {
    m = fseek((FILE *) fh, off, SEEK_SET);
    ior = (off == -1) ? (CELL) errno : 0;
  } else {
    ior = 0;
  }
;C

\ Return the current read/write offset
C: FILE-POSITION ( fh -- off ior ) 
  if(fh != stdin) {
    off = fseek((FILE *) fh, 0, SEEK_CUR);
    ior = (off == -1) ? (CELL) errno : 0;      
  } else {
    off = 0;
    ior = 0;
  }
;C


\ ---------- Block-based I/O ----------

\ Read characters from the given file into a buffer
C: READ-FILE ( addr n fh -- m ior )
  m = fread((CHARACTERPTR) addr, n, 1, (FILE *) fh);
  ior = (CELL) ferror((FILE *) fh);
;C

\ Write characters to the given file from the buffer
C: WRITE-FILE ( addr n fh -- ior )
  int m;
      
  m = fwrite((CHARACTERPTR) addr, n, 1, (FILE *) fh);
  ior = (CELL) ferror((FILE *) fh);
;C
     
      
\ ---------- Line-based I/O ----------

\ Read a line of text
C: READ-LINE prim_read_line ( addr n fh -- m f ior )
  CHARACTERPTR ptr;

  if(feof((FILE*) fh) || ((ptr = fgets((CHARACTERPTR) addr, n, (FILE *) fh)) == 0)) {
    m = 0;   f = 0;   ior = 0;
  } else {    
    m = strlen(ptr);
    f = 1;
    ior = (CELL) ferror((FILE *) fh);      
  }
;C

\ Write a line of text
C: WRITE-LINE ( addr n fh -- ior )
  fprintf((FILE *) fh, "%s\n", create_unix_string(addr, n));
  ior = (CELL) ferror((FILE *) fh);
;C


