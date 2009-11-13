\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007, UCD Dublin. All rights reserved.
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

\ Extra primitives needed to bootstrap the system.
\
\ These primitives are only intended for a bootstrapped Attila. Once
\ we have a running system using these, we can then use the cross-compiler
\ to re-generate the system from proper Attila source code.

C:
#include <stdio.h>
#include <string.h>
#include <readline/readline.h>

#define WHITESPACE " \t\n"

#define USER_STATUS 0
#define USER_LAST 1
#define USER_INPUTSOURCE 2
#define USER_TIB 3
#define USER_OFFSET 4

// The available numeric digits
static const char *digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

// The include path for source files
char **include_path;
int includes = 0;

// Assorted hacks to call underlying primitives from C level

extern PRIMITIVE user_variable_address;    
CELLPTR
user_variable( int n ) {
    PUSH_CELL(n);
    user_variable_address();
    return (CELLPTR) POP_CELL();
}    

extern PRIMITIVE to_name;
BYTEPTR
xt_to_name( XT xt, BYTEPTR *addr, CELL *len ) {
    PUSH_CELL(xt);
    to_name();
    (*len) = POP_CELL();
    (*addr) = (BYTEPTR) POP_CELL();
}

extern PRIMITIVE to_lfa;
CELLPTR
xt_to_lfa( XT xt ) {
    PUSH_CELL(xt);
    to_lfa();
    return (CELLPTR) POP_CELL();
}
;C


\ ---------- Terminal I/O ----------

\ Read a line into the text buffer from the input source. If the input
\ source is a terminal, use readline(); otherwise, just read the line.
\ Leave a flag on the stack, 0 if the input source is exhausted
PRIMITIVE: REFILL fill_tib ( -- f )
  char *line = NULL;
  FILE *input_source = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  char **tib = (char **) user_variable(USER_TIB);
  int *offset = (int *) user_variable(USER_OFFSET);

  if(input_source == stdin) {
    // use readline() to allow line editing, copy the first non-blank
    // line read to the TIB
    do {
      if(line != NULL) free(line);
      printf(" ok ");
      line = readline("");
    } while((line != NULL) &&
	    (strlen(line) == 0));
    if(line != NULL) {
      strcpy(*tib, line);   free(line);
      f = 1;
    }
  } else {
    // read directly into the TIB
    do {
      line = fgets(*tib, TIB_SIZE, input_source);
    } while((line != NULL) &&
	    (strlen(line) == 0));
    if(line == NULL)
      **tib = '\0';
    else
      f = 1;
  }
   
  // reset input to the start of the TIB
  *offset = 0;
;PRIMITIVE


\ Place the address of the current input point in the TIB and the
\ number of remaining characters onto the stack
PRIMITIVE: SOURCE ( -- addr n )
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int offset = (int) (*(user_variable(USER_OFFSET)));
  char *point = ((offset == -1) ? tib : tib + offset);
  n = (offset == -1) ? 0 : strlen(point);
;PRIMITIVE


\ Parse the next word, consuming leading whitespace. Leave either
\ an address count pair or zero on the stack
PRIMITIVE: PARSE-WORD ( -- )
  CELL c = 0;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  int newoffset;
  int len;
  char *newpoint;
  CELL remaining;

  do {
    // fill the TIB if we need a line
    if(*offset == -1) {
      fill_tib();

      // check for an exhausted input source, and fail if we have one
      remaining = POP_CELL();
      if(!remaining) {
	PUSH_CELL(remaining);
	return;
      }
    }
    
    // consume leading whitespace
    len = strspn(tib + *offset, WHITESPACE);
    *offset += len;
  
    // parse everything except whitespace
    len = strcspn(tib + *offset, WHITESPACE);
    if(len == 0)
      *offset = -1;
  } while(len == 0);
  newoffset = *offset + len;
  if(*(tib + newoffset) == '\0')
    newoffset = -1;
  
  // push the result onto the stack
  newpoint = tib + *offset;
  PUSH_CELL(newpoint);
  PUSH_CELL(len);

  // update point
  *offset = newoffset;
;PRIMITIVE


\ Consume all instances of the delimiter character in the input line
PRIMITIVE: CONSUME ( c -- )
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char cc;

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib();

    // check for an exhausted input source
    remaining = POP_CELL();
    if(!remaining)
      return;
  }

  while(cc = *(tib + *offset),
	((cc != '\0') &&
	 (cc == (char) c)))
    (*offset)++;
;PRIMITIVE


\ Parse the input stream up to the next instance of the delimiter
\ character, returning null if there is no such delimiter before
\ the end of the line
PRIMITIVE: PARSE ( c -- )
  char *ptr, *end;
  int len;
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char *point;

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib();

    // check for an exhausted input source, and fail if we have one
    remaining = POP_CELL();
    if(!remaining) {
      PUSH_CELL(remaining);
      return;
    }
  }

  // search for the next delimiter character
  point = tib + *offset;
  ptr = index(point, c);
  if(ptr == NULL) {
    PUSH_CELL(ptr);
    return;
  }

  // push the result onto the stack
  len = ptr - point;
  PUSH_CELL(point);
  PUSH_CELL(len);

  // update point
  *offset += len + 1;
  if(*(tib + *offset) == '\0')
    *offset = -1;
;PRIMITIVE


\ Print the given string on the terminal
PRIMITIVE: TYPE ( addr n -- )
  char *buf;

  // copy string to null-terminated local buffer
  buf = (char *) calloc(sizeof(CHARACTER), n + 1);
  strncpy(buf, (BYTEPTR) addr, len);
  buf[len] = '\0';

  // print and tidy up
  printf("%s", buf);   fflush(stdout);
  free(buf);
;PRIMITIVE


\ Print a character on the terminal
PRIMITIVE: EMIT ( c -- )
  printf("%c", (CHARACTER) c);
;PRIMITIVE


\ ---------- Compilation support ----------

\ Convert a token to a number if possible, pushing the result
\ (if done) and a flag
PRIMITIVE: NUMBER? ( addr n --- )
  char *buf;
  char *digitptr;
  int i, len, acc, digit;
  int valid = 1;
  int base = *(user_variable(USER_BASE));

  buf = til_string_to_c_string(addr, n);
  len = strlen(buf);

  // convert number
  acc = 0;
  for(i = 0; i < len; i++) {
    digitptr = index(digits, toupper(buf[i]));
    digit = digitptr - digits;
    if((digitptr == NULL) ||
       (digit > base)) {
      // illegal number character
      valid = 0;
      break;
    }
    acc = (acc * base) + digit;
  }

  if(valid)
    PUSH_CELL(acc);
  PUSH_CELL(valid);
  free(buf);
;PRIMITIVE

\ Look up a word in a list, traversing the headers until we find the word
\ or hit null
PRIMITIVE: (FIND) ( addr namelen start -- xt ) " bracket find"
    CELL taddr, tlen;
    CELLPTR link;
	
    xt = NULL;
    while(start != NULL) {
	xt_to_name(start, &taddr, &tlen);
	if(strncmp(addr, taddr, namelen) == 0) {
	    xt = start;   start = NULL;
	} else {
	    link = xt_to_lfa(start);
	    start = *link;
	}
    }
;PRIMITIVE	


\ ---------- File I/O ----------

\ Test if the input source has been exhausted, i.e. we can't do a REFILL
PRIMITIVE: EXHAUSTED? ( -- eof )
  FILE *f =  (FILE *) (*(user_variable(USER_INPUTSOURCE)));

  if(f == stdin)
    eof = 0;
  else {
    eof = feof(f);
  }
;PRIMITIVE


\ Open a file, returning a file handle or 0
PRIMITIVE: FILE-OPEN ( addr namelen -- fh )
  char *fn;

  fn = til_string_to_c_string((char *) addr, namelen);
  fh = (CELL) fopen(fn, "r");
  free(fn);
;PRIMITIVE


\ Close a file
PRIMITIVE: FILE-CLOSE ( fh -- )
  if(fh != stdin)
    fclose(fh);
;PRIMITIVE

