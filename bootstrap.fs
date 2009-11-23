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
\
\ All data -- headers, code and data -- are placed linearly in memory
\ in a single pre-allocated block. Each word is laid out as follows:
\
\     xt -> cf       code field             1 CELLS  bytes
\           len      length of name         1 byte
\           name     name of word           len bytes
\    lfa -> link     xt of previous word    1 CELLS bytes
\           status   status of word         1 byte
\           body     body of word           n cells
\
\ This isn't an especially  sensible layout, but it *is* simple for
\ bootstrapping purposes.

C:
#include "bootstrap.h"

// ---------- Initialisation ----------

BYTEPTR
init_memory( CELL len ) {
    return (BYTEPTR) malloc(len);
}


// ---------- Assorted hacks to call underlying primitives from C level ----------

VOID user_variable_address( XT );    
CELLPTR
user_variable( int n ) {
    PUSH_CELL(n);
    user_variable_address(NULL);
    return (CELLPTR) POP_CELL();
}    

VOID to_lfa( XT );
XTPTR
xt_to_lfa( XT xt ) {
    PUSH_CELL(xt);
    to_lfa(NULL);
    return (XTPTR) POP_CELL();
}

VOID to_status( XT );
BYTEPTR
xt_to_status( XT xt ) {
    PUSH_CELL(xt);
    to_status(NULL);
    return (BYTEPTR) POP_CELL();
}

VOID to_body( XT );
CELLPTR
xt_to_body( XT xt ) {
    PUSH_CELL(xt);
    to_body(NULL);
    return (CELLPTR) POP_CELL();
}

XT lastxt;
VOID execute( XT );
VOID docolon( XT );

VOID start_word( XT );
VOID to_status( XT );
VOID
begin_colon_definition( char *name, PRIMITIVE prim, int status) {
    XT xt;
    BYTEPTR addr;

    // compile the header
    PUSH_CELL(name);
    PUSH_CELL(strlen(name));
    PUSH_CELL(prim);
    start_word(NULL);
    xt = (XT) POP_CELL();

    // set status
    PUSH_CELL(xt);
    to_status(NULL);
    addr = (BYTEPTR) POP_CELL();
    *addr = status;

    // store the xt
    lastxt = xt;
    printf("%s %lx\n", name, xt);
}

VOID
end_colon_definition() {
}

VOID prim_compile_cell( XT );
VOID
compile_cell( CELL c ) {
    PUSH_CELL(c);
    prim_compile_cell(NULL);
}

VOID prim_compile_char( XT );
VOID
compile_byte( BYTE b ) {
    PUSH_CELL((CELL) b);
    prim_compile_char(NULL);
}

VOID prim_compile_string( XT );
VOID
compile_string( char *str, int len ) {
    PUSH_CELL((CELL) str);
    PUSH_CELL((CELL) len);
    prim_compile_string(NULL);
}

VOID bracket_find( XT );
XT xt_of( char *name ) {
    XT xt;
    CELL f;
        
    PUSH_CELL(name);
    PUSH_CELL((CELL) strlen(name));
    PUSH_CELL(*(user_variable(USER_LAST)));
    bracket_find(NULL);

    f = POP_CELL();
    if(!f) {
        printf("FAILED TO FIND %s\n", name);
        exit(1);
    } else {
        xt = POP_CELL();
        return xt;
    }
}
VOID
compile_xt_of( char *name ) {
    XT xt;

    xt = xt_of(name); 
    compile_cell((CELL) xt);
}

VOID allot( XT );
CELLPTR
allocate_code_memory( int n ) {
    CELLPTR ptr;

    ptr = (CELLPTR) top;
    PUSH_CELL((CELL) n);
    allot(NULL);
    return ptr;
}

void
show_execute( XT xt ) {
    CHARACTERPTR buf;
    BYTEPTR ha;
    CELL len;

    if(xt != 0) {
        ha = (BYTEPTR) ((CELLPTR) xt + 1);
        len = (BYTE) (*ha);
        buf = (CHARACTERPTR) malloc(len + 1);
        memcpy(buf, ha + 1, len);   buf[len] = '\0';
        printf("%s\n", buf);
        free(buf);
    }
}
;C


\ ---------- Memory and compilation primitives ----------

\ We don't do alignment for boostrapping
PRIMITIVE: ALIGNED ( addr -- addr )
;PRIMITIVE
PRIMITIVE ALIGN ( -- )
;PRIMITIVE

\ Compile a character into the body of a word
PRIMITIVE: C, compile_char ( c -- )
    *top++ = (BYTE) c;
;PRIMITIVE

\ Compile a cell into the body of a word
PRIMITIVE: , ( v -- )
    CELLPTR t;

    t = (CELLPTR) top;
    *t = (CELL) v;
    top += CELL_SIZE;
;PRIMITIVE

\ Allocate some body memory
PRIMITIVE: ALLOT allot ( n -- )
    top += n;
;PRIMITIVE

\ Return the address of the top of the dictionary
PRIMITIVE: TOP ( -- addr )
    addr = (CELL) top;
;PRIMITIVE

\ Compile a cell (generally an xt) into the code of a word
PRIMITIVE: COMPILE, prim_compile_cell ( xt -- )
    CELLPTR t;

    t = (CELLPTR) top;
    *t = (CELL) xt;
    top += CELL_SIZE;
;PRIMITIVE

\ Compile a character into the code of a word
PRIMITIVE: CCOMPILE, prim_compile_char ( c -- )
    *top++ = (BYTE) c;
;PRIMITIVE

\ Compile a string into the code of a word
PRIMITIVE: SCOMPILE, prim_compile_string ( addr n -- )
    int i;
    CHARACTERPTR str;

    str = (CHARACTERPTR) addr;
    compile_byte(n);
    for(i = 0; i < n; i++ ) {
        compile_byte(str[i]);
   }
;PRIMITIVE

\ Return the address of the next available body word. This is the
\ same as TOP in this memory model, but conceptually distinct
PRIMITIVE: HERE ( -- addr )
    addr = (CELL) top;
;PRIMITIVE

\ Return the address of the n'th user variable
\ sd: note USERVAR not USER -- the latter is the new-user-variable-creating word
PRIMITIVE: USERVAR user_variable_address ( n -- addr )
    addr = (CELL) (((CELLPTR) user) + n);
;PRIMITIVE


\ ---------- Navigating the header ----------

\ Covert an xt to a code field address.
PRIMITIVE: >CFA ( xt -- xt )
;PRIMITIVE

\ Convert an xt to a link field address
PRIMITIVE: >LFA to_lfa ( xt -- lfa )
    BYTEPTR ptr;

    ptr = (BYTEPTR) xt;
    ptr += CELL_SIZE;
    lfa = (CELL) (ptr + 1 + *ptr);
;PRIMITIVE

\ Convert an xt to a status byte address.
PRIMITIVE: >STATUS to_status ( -- addr )
    CELLPTR lfa;

    to_lfa(NULL);
    lfa = (CELLPTR) POP_CELL();
    addr = (CELL) (lfa + 1);
;PRIMITIVE

\ Convert an xt to the body address, accounting for the indirect
\ boy address that's stored for CREATEd words 
PRIMITIVE: >BODY to_body ( -- addr ) " to body"
    BYTEPTR st;
    BYTE status;

    to_status(NULL);
    st = (BYTEPTR) POP_CELL();
    status = (BYTE) *st;
    addr = (CELL) (st + 1);
    if(status & STATUS_REDIRECTABLE)
        addr += CELL_SIZE;
;PRIMITIVE

\ Convert an xt to the name of the corresponding word
PRIMITIVE: >NAME to_name ( xt -- addr namelen)
    BYTEPTR nla;

    nla = (BYTEPTR) ((CELLPTR) xt + 1);
    namelen = (BYTE) *nla;
    addr = (CELL) (nla + 1);
;PRIMITIVE


\ ---------- Control primitives ----------

\ Jump unconditionally to the address compiled in the next cell
PRIMITIVE: (BRANCH) ( -- )
  CELL offset = (CELL) *ip;
  ip = (XTPTR) (((BYTEPTR) ip) + offset);
;PRIMITIVE

\ Test the top of the stack and either continue (if true) or
\ jump to the address compiled in the next cell (if false)
PRIMITIVE: (?BRANCH) ( f -- )
  if(f)
    ip++;
  else {
    CELL offset = (CELL) *ip;
    ip = (XTPTR) (((BYTEPTR) ip) + offset);
  }
;PRIMITIVE


\ ---------- Inner interpreter ----------

\ Execute an xt from the stack
PRIMITIVE: EXECUTE execute ( xt -- )
    PRIMITIVE prim;

    // dispatch on the instruction
    if(xt == (XT) NULL) {
        // NEXT, return from this word
	ip = POP_RETURN();
    } else {
	prim = (PRIMITIVE) (*((CELLPTR) xt));
	if(prim == docolon) {
	    // another colon-definition, push the return address
	    // and re-point the instruction pointer
	    PUSH_RETURN(ip);
            ip = (XTPTR) xt_to_body((XT) xt);
	} else {
	    // a primitive, execute it
	    (*prim)(xt);
	}
    }
;PRIMITIVE

\ Run the virtual machine interpretation loop. This is simple, infinite,
\ single-threaded loop to get things going
PRIMITIVE: (:) docolon ( -- ) " bracket colon"
    XT xt;
	
    do {
	// grab the next instruction
	xt = (*((XTPTR) ip++));

	// run the debug code if we're tracing
	if(*(user_variable(USER_TRACE))) {
            DEBUG(xt);
	}
		
	// EXECUTE it
	PUSH_CELL(xt);
	execute(xt);
    } while(1);
;PRIMITIVE

\ The run-time behaviour of a variable, returning its body address
PRIMITIVE: (VAR) dovar ( -- addr ) " bracket var"
    addr = (CELL) xt_to_body(_xt);
;PRIMITIVE

\ For a re-directable word, grab the indirect body address and jump to it,
\ pushing the real body address onto the stack first
\ sd: should we combine this with (VAR) and switch on redirectability?
PRIMITIVE: (DOES) ( -- body )
    CELLPTR iba;
	
    body = (CELL) xt_to_body(_xt);
    iba = (CELLPTR) ((CELLPTR) body - 1);
    PUSH_RETURN(ip);
    ip = (XT) *((CELLPTR *) iba);
;PRIMITIVE


\ ---------- Word compilation ----------

\ Compile a word header. Note that this has to work with an empty name
PRIMITIVE: (HEADER,) start_word ( addr len cf -- xt )
    XT last;
	
    xt = (XT) top;                                     // grab the xt	
    *((CELLPTR) top) = cf;
    top += CELL_SIZE;
    *top++ = (BYTE) len;                               // the counted-string name   
    memcpy(top, (BYTEPTR) addr, len);   top += len;
    *((CELLPTR) top)  = (XT) *(user_variable(USER_LAST));   // the link pointer	
    top += CELL_SIZE;
    *top++ = (BYTE) 0;                                 // the status field
    *(user_variable(USER_LAST)) = xt;                  // LAST gets the xt we just compiled
;PRIMITIVE


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
      fill_tib(_xt);

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
    fill_tib(_xt);

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
    fill_tib(_xt);

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
  buf = (CHARACTERPTR) malloc(n + 1);
  memcpy(buf, (BYTEPTR) addr, n);   buf[n] = '\0';

  // print and tidy up
  printf("%s", buf);   fflush(stdout);
  free(buf);
;PRIMITIVE

\ Print a character on the terminal
PRIMITIVE: EMIT ( c -- )
  printf("%c", (CHARACTER) c);
;PRIMITIVE

\ Print the top number on the stack
PRIMITIVE: . ( n -- )	
    printf("%d", n);
;PRIMITIVE

\ Print the whole stack
PRIMITIVE: .S ( -- )
    int i, n;

    n = DATA_STACK_DEPTH();
    printf("#%d", n);
    for(i = n - 1; i >= 0; i--) {
        printf(" %d", *(DATA_STACK_ITEM(i)));
    }
;PRIMITIVE

	
\ ---------- Compilation support ----------

\ Convert a token to a number if possible, pushing the result
\ (if done) and a flag
PRIMITIVE: NUMBER? ( addr n -- )
  char *buf;
  char *digitptr;
  int i, len, acc, digit;
  int valid = 1;
  int base = *(user_variable(USER_BASE));

  buf = (CHARACTERPTR) malloc(n + 1);
  memcpy(buf, addr, n);   buf[n] = '\0';

  // convert number
  acc = 0;
  for(i = 0; i < n; i++) {
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
\ or hit null. We return 0 if the word is not found, its xt and 1 if it
\ is, and its xt and -1 if it is found an is immediate
PRIMITIVE: (FIND) bracket_find ( addr namelen x -- ) " bracket find"
    CHARACTERPTR taddr;
    BYTEPTR ha;
    CELL tlen;
    XT xt;
    CELLPTR link;
    BYTE status;
	
    xt = (XT) NULL;
    while(x != (BYTEPTR) NULL) {
        ha = ((BYTEPTR) x + CELL_SIZE);
	tlen = (BYTE) *((BYTEPTR) ha);
	taddr = (CHARACTERPTR) ha + 1;
	if((namelen == tlen) &&
	   (strncasecmp(addr, taddr, namelen) == 0)) {
	    xt = x;   x = NULL;
	} else {
	    link = xt_to_lfa(x);
	    x = (XT) (XTPTR) (*link);	}
    }

    PUSH_CELL(xt);
    if(xt != (XT) NULL) {
        status = *xt_to_status(xt);
        PUSH_CELL((status & STATUS_IMMEDIATE) ? -1 : 1);
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
  CHARACTERPTR fn;

  fn = (CHARACTERPTR) malloc(namelen + 1);
  strncpy(fn, (CHARACTERPTR) addr, namelen);   fn[namelen] = '\0';
  fh = (CELL) fopen(fn, "r");
  free(fn);
;PRIMITIVE

\ Close a file
PRIMITIVE: FILE-CLOSE ( fh -- )
  if(((FILE *) fh) != stdin)
    fclose((FILE *) fh);
;PRIMITIVE


\ ---------- Start-up and shut-down ----------

\ Warm-start the system into a known state
PRIMITIVE: WARM ( -- )
    // reset the stacks      
    DATA_STACK_RESET();
    RETURN_STACK_RESET();  

    // reset user variables
    *(user_variable(USER_STATE)) = (CELL) STATE_INTERPRETING;
    *(user_variable(USER_BASE)) = (CELL) 10;
    *(user_variable(USER_TRACE)) = (CELL) 0;
    *(user_variable(USER_INPUTSOURCE)) = stdin;
    *(user_variable(USER_OFFSET)) = -1; // in need of a refill
      
    // point the ip at the executive and return
    ip = xt_to_body(*(user_variable(USER_EXECUTIVE)));
;PRIMITIVE
      