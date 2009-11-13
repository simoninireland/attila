// $Id$

// This file is part of Attila, a multi-targeted threaded interpreter
// Copyright (c) 2007, UCD Dublin. All rights reserved.
//
// Attila is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// Attila is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111, USA.

// Interaction primitives
//
// Additional primitives allowing terminal and file I/O.

#include "attila.h"
#include <readline/readline.h>

#define WHITESPACE " \t\n"

// The available numeric digits
static const char *digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

// The include path for source files
char **include_path;
int includes = 0;


// ---------- Terminal I/O ----------

// Read a line into the text buffer from the input source. If the input
// source is a terminal, use readline(); otherwise, just read the line.
// Leave a flag on the stack, 0 if the input source is exhausted
// PRIMITIVE REFILL ( -- f )
void
fill_tib() {
  char *line = NULL;
  FILE *input_source = (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  char **tib = (char **) user_variable(USER_TIB);
  int *offset = (int *) user_variable(USER_OFFSET);
  CELL rc = 0;

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
      rc = 1;
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
      rc = 1;
  }
   
  // reset input to the start of the TIB
  *offset = 0;
  PUSH_CELL(rc);
}


// Place the address of the current input point in the TIB and the
// number of remaining characters onto the stack
// PRIMITIVE SOURCE ( -- addr n )
void
point_and_length() {
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int offset = (int) (*(user_variable(USER_OFFSET)));
  char *point = (offset == -1) ? tib : tib + offset;
  int len = (offset == -1) ? 0 : strlen(point);

  PUSH_CELL(point);
  PUSH_CELL(len);
}


// Parse the next word, consuming leading whitespace
// PRIMITIVE PARSE-WORD ( -- addr n | 0 )
void
parse_word() {
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
      POP_CELL(remaining);
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
}


// Consume all instances of the delimiter character in the input line
// PRIMITIVE CONSUME ( c -- )
void
consume() {
  CELL c;
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char cc;

  // grab the delimiter
  POP_CELL(c);

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib();

    // check for an exhausted input source
    POP_CELL(remaining);
    if(!remaining)
      return;
  }

  while(cc = *(tib + *offset),
	((cc != '\0') &&
	 (cc == (char) c)))
    (*offset)++;
}


// Parse the input stream up to the next instance of the delimiter
// character, returning null if there is no such delimiter before
// the end of the line
// PRIMITIVE PARSE ( c -- addr n | 0 )
void
parse() {
  CELL c;
  char *ptr, *end;
  int len;
  CELL remaining;
  char *tib = (char *) (*(user_variable(USER_TIB)));
  int *offset = (int *) user_variable(USER_OFFSET);
  char *point;

  // grab the delimiter
  POP_CELL(c);

  // fill the TIB if we need a line
  if(*offset == -1) {
    fill_tib();

    // check for an exhausted input source, and fail if we have one
    POP_CELL(remaining);
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
}


// Print the given string on the terminal
// PRIMITIVE TYPE ( addr n -- )
void
print_string() {
  char *buf;

  // copy string to null-terminated local buffer
  buf = (char *) calloc(sizeof(char), n + 1);
  strncpy(buf, addr, n);
  buf[n] = '\0';

  // print and tidy up
  printf("%s", buf);   fflush(stdout);
  free(buf);
}


// Print a character on the terminal
// PRIMITIVE EMIT ( c -- )
void
emit() {
  CELL c;
  POP_CELL(c);
  printf("%c", (char) c);
}


// ---------- Compilation support ----------

// Convert a token to a number if possible, pushing the result
// (if done) and a flag
// PRIMITIVE NUMBER? ( addr n --- v 1 | 0 )
void
convert_number() {
  char *ptr;
  int n;
  char *buf;
  char *digitptr;
  int i, len, acc, digit;
  int valid = 1;
  int base = *(user_variable(USER_BASE));

  POP_CELL(n);   POP_CELL(ptr);
  buf = til_string_to_c_string(ptr, n);
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
}


// Convert an ha to the name in TIL form
// PRIMITIVE HA>NAME ( ha -- addr len )
void
hha_to_name() {
  HA ha;
  char *addr;
  CELL len;
  POP_CELL(ha);
  len = ha->namelen;   addr = ha_to_name(ha);
  PUSH_CELL(addr);
  PUSH_CELL(len);
}


// Convert an ha to an xt
// PRIMITIVE HA> ( ha -- xt )
void
hha_to_xt() {
  CELL ha;
  XT xt;

  POP_CELL(ha);
  xt = ha_to_xt(ha);
  PUSH_CELL(xt);
}


// Convert an ha to an lfs
// PRIMITIVE HA>LFA ( ha -- lfa )
void
hha_to_lfa() {
  HA ha;
  HA *lfa;

  POP_CELL(ha);
  lfa = &(ha->lfa);
  PUSH_CELL(lfa);
}


// Traverse a chain of word headers looking for the sought word,
// returning its xt or null.
// PRIMITIVE (FIND) ( addr c ha -- xt | 0 )
void
bracket_find() {
  char *addr;
  int namelen;
  XT xt = NULL;
  HA ha;

  POP_CELL(ha);   POP_CELL(namelen);   POP_CELL(addr);
  while(ha != NULL) {
    if(namelen == ha->namelen) {
      if(strncasecmp(addr, ha_to_name(ha), ha->namelen) == 0) {
	xt = ha_to_xt(ha);
	PUSH_CELL(xt);
	return;
      }
    }
    ha = ha->lfa;
  }

  PUSH_CELL(xt);
}


// Create the header for a word starting at the address given, leaving
// its xt on the stack. The word's code field points to NOOP. The word
// is defined in the current vocabulary
// PRIMITIVE (HEADER,) ( addr n -- xt )
void
header() {
  char *name;
  int namelen;

  // read the word to create
  POP_CELL(namelen);
  POP_CELL(name);

  // compile the header for the word
  HA ha = begin_word(name, namelen);

  // return the xt
  XT xt = ha_to_xt(ha);
  PUSH_CELL(xt);
}


// Return the ha of the last word defined. Note that this accesses the current
// vocabulary, and will fail of this is changed between definition and access
// PRIMITIVE LASTHA ( -- ha )
void
lastha() {
  VA voc = (VA) *(user_variable(USER_CURRENT));
  CELL lastha = (CELL) voc->lastha;
  PUSH_CELL(lastha);
}


// Return the xt of the last word defined. Note that this accesses the current
// vocabulary, and will fail of this is changed between definition and access
// PRIMITIVE LASTXT ( -- xt )
void
lastxt() {
  VA voc = (VA) *(user_variable(USER_CURRENT));
  XT lastxt = (CELL) voc->lastxt;
  PUSH_CELL(lastxt);
}
  

// Change the CFA of the given word
// PRIMITIVE CFA, ( cf xt -- )
void
set_cfa() {
  XT xt;
  CELLPTR ptr;
  CELLPTR cfa;

  POP_CELL(xt);
  POP_CELL(ptr);
  xt_to_codeheader(xt)->cfa = (PRIMITIVE) ptr;
}


// ---------- File I/O ----------

// Test if the input source has been exhausted, i.e. we can't do a REFILL
// PRIMITIVE EXHAUSTED? ( -- f )
void
exhausted() {
  FILE *f =  (FILE *) (*(user_variable(USER_INPUTSOURCE)));
  CELL eof;

  if(f == stdin)
    eof = 0;
  else {
    eof = feof(f);
  }
  PUSH_CELL(eof);
}


// Open a file, returning a file handle or 0
// PRIMITIVE FILE-OPEN ( addr n -- fh | 0 )
void
file_open() {
  CELL namelen;
  CELL addr;
  char *fn;
  FILE *f;

  POP_CELL(namelen);
  POP_CELL(addr);
  fn = til_string_to_c_string((char *) addr, namelen);
  f = fopen(fn, "r");
  PUSH_CELL(f);
  free(fn);
}


// Close a file
// PRIMITIVE FILE-CLOSE ( fh -- )
void
file_close() {
  FILE *f;

  POP_CELL(f);
  if(f != stdin)
    fclose(f);
}


// Open a file, taking account of the include path
// PRIMITIVE FILE-OPEN-INCLUDE ( addr len -- fh | 0 )
void
file_open_include() {
  int i;
  CELL addr, namelen;
  char *fn;
  FILE *f;
  char buf[255];

  POP_CELL(namelen);
  POP_CELL(addr);

  // try it "bare" first
  fn = til_string_to_c_string((char *) addr, namelen);
  // printf("Including %s...\n", fn);
  f = fopen(fn, "r");
  free(fn);
  if(f) {
    PUSH_CELL(f);
    return;
  }

  // otherwise try all included directories
  for(i = 0; i < includes; i++) {
    strcpy(buf, include_path[i]);
    strcat(buf, "/");             // sd: is this portable?
    strncat(buf, addr, namelen);
    f = fopen(buf, "r");
    if(f)
      break;
  }
  PUSH_CELL(f);
}

