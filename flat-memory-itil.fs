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

\ An indirect threaded interpreter with a flat memory model. This
\ is the "normal" Forth model as found in figForth etc.
\
\ ALl data -- headers, code and data -- are placed linearly in memory
\ in a single pre-allocated block. Each word is laid out as follows:
\
\           name     name of word           len bytes
\           len      length of name         1 byte
\    lfa -> link     xt of previous word    1 CELLS bytes
\     xt -> cf       code field             1 CELLS  bytes
\           status   status of word         1 byte
\           bp       body pointer           1 CELLS bytes
\           code     code of word           n cells
\           body     body of word           m cells
\
\ The xt field is the execution token for the word, used to represent
\ it compiled.
\
\ (The reason for the slightly bizarre name layout is to allow us to
\ convert xt's into word names, for ease of debugging. This isn't available
\ in other memory models, and so shouldn't be relied upon.)

\ ---------- Memory and compilation primitives ----------

\ Compile a character into the body of a word
PRIMITIVE: C, ( c -- )
    (*(((BYTEPTR) top)++)) = (BYTE) c;
;PRIMITIVE

\ Compile a cell into the body of a word
PRIMITIVE: , ( v -- )
    (*(((CELLPTR) top)++)) = v;
;PRIMITIVE

\ Allocate some body memory
PRIMITIVE: ALLOT ( n -- )
    ((BYTEPTR) top) += n;
;PRIMITIVE

\ Return the address of the top of the dictionary
PRIMITIVE: TOP ( -- addr )
    addr = (CELL) top;
;PRIMITIVE

\ Compile an xt into the code of a word. In this memory model this is the
\ same as , but conceptually distinct
PRIMITIVE: COMPILE, ( xt -- )
    (*(((CELLPTR) top)++)) = xt;
;PRIMITIVE

\ Return the address of the next available body word. This is the
\ same as TOP in this memory model, but conceptually distinct
PRIMITIVE: HERE ( -- addr )
    addr = (CELL) top;
;PRIMITIVE

\ Return the address of the n'th user variable
PRIMITIVE: USER user_variable_address ( n -- addr )
    addr = user + n;
;PRIMITIVE


\ ---------- Inner interpreter ----------

\ Run the virtual machine interpretation loop. This is simple, infinite,
\ single-threaded loop to get things going
PRIMITIVE: (:) docolon ( -- ) " bracket colon"
    XT xt;
    PRIMITIVE prim;
    do {
	// grab the next instruction
	xt = (*ip);

	// dispatch on the instruction
	if(xt == NULL) {
	    // NEXT, return from this word
	    ip = POP_RETURN();
	} else {
	    prim = (*xt);   ip++;
	    if(prim == docolon) {
	        // another colon-definition, push the return address
	        // and re-point the instruction pointer
	        PUSH_RETURN(ip);
	        ip = (XTPTR) (((BYTEPTR) xt) + 1 + size(CELL) * 2);
	    } else {
	        // a primitive, execute it
	        (*prim)();
	    }
    } while(1);
;PRIMITIVE
	    
\ Execute an xt from the stack
PRIMITIVE: EXECUTE ( xt -- )
    PUSH_RETURN(xt);
;PRIMITIVE
	    
	    
\ ---------- Word compilation ----------

\ Start a word, compiling a header for it and setting it up for
\ its code and data. The resulting header isn't complete, and needs
\ to be finalised with a corresponding WORD) before use.
: (WORD ( addr len -- xt )
    LASTXT ,        \ the link pointer
    DUP >R          \ the name
    DO
	DUP C@ C<
	1+
    LOOP
    R> C,          \ the length of the name
    TOP            \ ( xt )
    [ ' (:) ] ,    \ the code field
    INITIALISED C, \ the initial status
    0 C, ;         \ the body pointer, to be patched

\ Finish a word's compilation, patching its body pointer and setting
\ its status to findable, and updating LAST.
: WORD) ( xt -- )
    DUP LAST !      \ LAST gets the xt
    TOP OVER >BPA ! \ patch body address to current top
    READY SWAP ! ;  \ patch status to ready and findable


\ ---------- Navigating the header ----------

\ Covert an xt to a code field address. In this memory model that's simply
\ a no-op.
: >CFA ( xt -- cfa ) ;

\ Convert an xt to a body pointer address. This shouldn't be used by
\ applications.
: >BFA ( xt -- bfa)
    1+ 1 CELLS + ;

\ Convert an xt to a body address. This is the application-level word
\ that can be used portably.
: >BODY ( xt -- addr )
    >BFA @ ;

\ Convert an xt to a status byte address.
: >STATUS ( xt -- caddr )
    1+ ;

\ Convert an xt to a code address.
: >CODE ( xt -- addr )
    1+ 2 CELLS + ;

\ Convert an xt to the name of the corresponding word. This is not available
\ in all memory models, but does make debugging easier.
: >NAME to_name ( xt -- addr len )
    1 CELLS - 1- DUP C@         ( laddr l )
    SWAP OVER - 1- ;

\ Convert an xt to a link field address containing the xt of the next word
\ in the search list.
: >LFA to_lfa ( xt -- lfa )
    1 CELLS - ;

