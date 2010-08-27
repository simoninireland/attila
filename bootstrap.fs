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

CHEADER:
#include "bootstrap.h"

// ---------- Initialisation ----------

BYTEPTR
init_memory( CELL len ) {
    return (BYTEPTR) malloc(len);
}

XT master_executive = NULL;
jmp_buf env;


// ---------- Assorted hacks to call underlying primitives from C level ----------

CHARACTERPTR
create_unix_string( CELL addr, CELL namelen ) {
  static CHARACTER namebuf[256];

  strncpy(namebuf, (CHARACTERPTR) addr, namelen);   namebuf[namelen] = '\0';
  return namebuf;
}

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
    // printf("%s %lx\n", name, xt);
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
;CHEADER


\ ---------- Memory and compilation primitives ----------

\ We don't do alignment for boostrapping
C: ALIGNED ( addr -- addr )
;C
C: ALIGN ( -- )
;C
C: CALIGNED ( addr -- addr )
;C
C: CALIGN ( -- )
;C

\ Compile a character into the body of a word
C: C, compile_char ( c -- )
    *top++ = (BYTE) c;
;C
    
\ Compile a cell into the body of a word
C: , prim_memory_cell ( v -- )
    CELLPTR t;

    t = (CELLPTR) top;
    *t = (CELL) v;
    top += CELL_SIZE;
;C

\ Compile an xt into the body of a word
C: XT, ( -- )
    prim_memory_cell(_xt);
;C
    
\ Allocate some body memory
C: ALLOT allot ( n -- )
    top += n;
;C

\ Compile a cell (generally an xt) into the code of a word
C: COMPILE, prim_compile_cell ( xt -- )
    CELLPTR t;

    t = (CELLPTR) top;
    *t = (CELL) xt;
    top += CELL_SIZE;
;C

\ Compile a character into the code of a word
C: CCOMPILE, prim_compile_char ( c -- )
    *top++ = (BYTE) c;
;C

\ Compile a string into the code of a word
C: SCOMPILE, prim_compile_string ( addr n -- )
    int i;
    CHARACTERPTR str;

    str = (CHARACTERPTR) addr;
    compile_byte(n);
    for(i = 0; i < n; i++ ) {
        compile_byte(str[i]);
   }
;C

\ Compile an xt into the code of a word -- same as COMPILE,
C: XTCOMPILE, ( -- )
    prim_compile_cell(_xt);
;C

\ Compile a cfa into the code of a word -- same as COMPILE,
C: CFACOMPILE, ( -- )
    prim_compile_cell(_xt);
;C

\ Compile a ct -- same as XTCOMPILE, in an indirect threaded interpreter
C: CTCOMPILE, ( -- )
    prim_compile_cell(_xt);
;C

\ Compile NEXT
C: NEXT, ( -- )
    PUSH_CELL(0);
    prim_compile_cell(_xt);
;C
    
\ Return the address of the n'th user variable
\ sd: note USERVAR not USER -- the latter is the new-user-variable-creating word
C: USERVAR user_variable_address ( n -- addr )
    addr = (CELL) (((CELLPTR) user) + n);
;C
    
\ Return the address of the next available body word. This is the
\ same as TOP in this memory model, but conceptually distinct
C: HERE ( -- addr )
    addr = (CELL) top;
;C

\ Return the address of the top of the dictionary
C: TOP ( -- addr )
    addr = (CELL) top;
;C


\ ---------- Navigating the header ----------

\ Convert an xt to a code field address.
C: >CFA ( xt -- xt )
;C

\ Retrieve the code field of an xt
C: CFA@ ( xt -- cfa )
    cfa = *((CELLPTR *) xt);
;C
    
\ Update the code field of an xt
C: CFA! ( cfa xt -- )
    *((CELLPTR) xt) = cfa;
;C
    
\ Convert an xt to a link field address
C: >LFA to_lfa ( xt -- lfa )
    BYTEPTR ptr;

    ptr = (BYTEPTR) xt;
    ptr += CELL_SIZE;
    lfa = (CELL) (ptr + 1 + *ptr);
;C

\ Convert an xt to a status byte address.
C: >STATUS to_status ( -- addr )
    CELLPTR lfa;

    to_lfa(NULL);
    lfa = (CELLPTR) POP_CELL();
    addr = (CELL) (lfa + 1);
;C
    
\ Convert an xt to the body address, accounting for the indirect
\ boy address that's stored for CREATEd words 
C: >BODY to_body ( -- addr )
    BYTEPTR st;
    BYTE status;

    to_status(NULL);
    st = (BYTEPTR) POP_CELL();
    status = (BYTE) *st;
    addr = (CELL) (st + 1);
    if(status & STATUS_REDIRECTABLE)
        addr += CELL_SIZE;
;C

\ Convert an xt to its indirect body address (without checking
\ whether this makes sense
C: >IBA ( -- addr )
    to_body(_xt);
    addr = POP_CELL();
    addr = (CELL) (((BYTEPTR) addr) - CELL_SIZE);
;C
    
\ Convert an xt to the name of the corresponding word
C: >NAME to_name ( xt -- addr namelen)
    BYTEPTR nla;

    nla = (BYTEPTR) ((CELLPTR) xt + 1);
    namelen = (BYTE) *nla;
    addr = (CELL) (nla + 1);
;C


\ ---------- Literals ----------

\ Push the next cell in the instruction stream as a literal
C: (LITERAL) ( -- l )
    CELLPTR addr;

    addr = (CELLPTR) ip;
    l = (*addr);
    ip++;
;C

\ Push a string in the code space onto the stack as a standard
\ address-plus-count pair
C: (SLITERAL) ( -- s n )
    BYTEPTR addr;

    addr = (BYTEPTR) ip;
    n = (CELL) *addr;
    s = addr + 1;
    ip = (XTPTR) ((BYTEPTR) ip + n + 1);
;C


\ ---------- Control primitives ----------

\ Jump unconditionally to the address compiled in the next cell
C: (BRANCH) ( -- )
  CELL offset = (CELL) *ip;
  ip = (XTPTR) (((BYTEPTR) ip) + offset);
;C

\ Test the top of the stack and either continue (if true) or
\ jump to the address compiled in the next cell (if false)
C: (?BRANCH) ( f -- )
  if(f)
    ip++;
  else {
    CELL offset = (CELL) *ip;
    ip = (XTPTR) (((BYTEPTR) ip) + offset);
  }
;C


\ ---------- Inner interpreter ----------

\ Execute an xt from the stack
C: EXECUTE execute ( xt -- )
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
;C

\ Run the virtual machine interpretation loop. This is simple, infinite,
\ single-threaded loop to get things going
C: (:) docolon ( -- ) " bracket colon"
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
;C

\ The run-time behaviour of a variable, returning its body address
C: (VAR) dovar ( -- addr ) " bracket var"
    addr = (CELL) xt_to_body(_xt);
;C

\ For a re-directable word, grab the indirect body address and jump to it,
\ pushing the real body address onto the stack first
\ sd: should we combine this with (VAR) and switch on redirectability?
C: (DOES) ( -- body )
    CELLPTR iba;
	
    body = (CELL) xt_to_body(_xt);
    iba = (CELLPTR) ((CELLPTR) body - 1);
    PUSH_RETURN(ip);
    ip = (XT) *((XTPTR *) iba);
;C


\ ---------- Word compilation ----------

\ Compile a word header. Note that this has to work with an empty name
C: (HEADER,) start_word ( addr len cf -- xt )
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
;C

\ Look up a word in a list, traversing the headers until we find the word
\ or hit null. We return 0 if the word is not found, its xt and 1 if it
\ is, and its xt and -1 if it is found an is immediate
C: (FIND) bracket_find ( addr namelen x -- )
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
	if((((*xt_to_status(x)) & STATUS_HIDDEN) == 0) &&
	   (namelen == tlen) &&
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
;C


\ ---------- Start-up and shut-down ----------

\ Warm-start the system into a known state
C: WARM ( -- )
    setjmp(env);
	    
    // reset the stacks      
    DATA_STACK_RESET();
    RETURN_STACK_RESET();  

    // reset user variables
    *(user_variable(USER_STATE)) = (CELL) STATE_INTERPRETING;
    *(user_variable(USER_BASE)) = (CELL) 10;
    *(user_variable(USER_TRACE)) = (CELL) 0;
    if(*(user_variable(USER_INPUTSOURCE)) != NULL &&
       *(user_variable(USER_INPUTSOURCE)) != stdin)
        fclose(*(user_variable(USER_INPUTSOURCE)));
    *(user_variable(USER_INPUTSOURCE)) = stdin;
    *(user_variable(USER__IN)) = -1; // in need of a refill
    if(*(user_variable(USER_OUTPUTSINK)) != NULL &&
       *(user_variable(USER_OUTPUTSINK)) != stdout)
        fclose(*(user_variable(USER_OUTPUTSINK)));
    *(user_variable(USER_OUTPUTSINK)) = stdout;
    if(master_executive == NULL)
        master_executive = xt_of("OUTER");
    *(user_variable(USER_EXECUTIVE)) = (CELL) master_executive;
	
    // point the ip at the executive and return
    ip = xt_to_body(*(user_variable(USER_EXECUTIVE)));
;C

\ Exit the system
C: BYE ( -- )
    exit(0);
;C