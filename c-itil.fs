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

\ An indirect-threaded inner interpreter
\
\ This code is independent of memory model except that it
\ expects three primitives to be defined:
\
\  - xt_to_body  >BODY
\  - xt_to_cfa   >CFA
\  - xt_to_iba   >IBA

\ ---------- Very low-level debugging suppprt ----------

CHEADER:
#ifdef DEBUGGING
static int indent;
static char buf[256];

static void print_word_name( XT xt ) {
   XT _xt;
   int i;
   CELL n;
   BYTEPTR addr;

   PUSH_CELL(xt);
   CALL(xt_to_name);
   n = POP_CELL();
   addr = (BYTEPTR) POP_CELL();
   strncpy(buf, addr, n);   buf[n] = '\0';
   for(i = 0; i < indent; i++) printf(" ");
      // printf("%x %s ", (CELL) (((BYTEPTR) xt - (BYTEPTR) image) / sizeof(CELL)), buf);
      printf("%s ", buf);
}

static void print_data_stack() {
   int i;
   printf("( ");
   for(i = DATA_STACK_DEPTH() - 1; i >= 0; i--)
      printf("%d ", *(DATA_STACK_ITEM(i)));
   printf(")");
}

static void print_return_stack() {
   int i;
   printf("( ");
   for(i = RETURN_STACK_DEPTH() - 1; i >= 0; i--)
      printf("%d ", *(RETURN_STACK_ITEM(i)));
   printf(")");
}
#endif
;CHEADER

\ ---------- The inner interpreter ---------- 

\ Execute an xt from the stack
C: EXECUTE execute ( xt -- )
    PRIMITIVE prim;

    #ifdef DEBUGGING
        // print word
	int i;
	if(xt == (XT) NULL) { 
            indent -= 3;
        } else {
            printf("%d: %d ", (ip - 1), xt);
            print_word_name(xt);
	    printf(" ");
	    print_data_stack();
            printf(" R: ");
	    print_return_stack();
	    printf("\n");
            if((PRIMITIVE) (*((CELLPTR) xt)) == (PRIMITIVE) docolon)
                indent += 3;
	}
    #endif
	
    // dispatch on the instruction
    if(xt == (CELL) NULL) {
        // NEXT, return from this word
	ip = (XTPTR) POP_RETURN();
    } else {
	prim = (PRIMITIVE) (*((CELLPTR) xt));
	if(prim == (PRIMITIVE) docolon) {
	    // another colon-definition, push the return address
	    // and re-point the instruction pointer
            PUSH_RETURN(ip);
            PUSH_CELL(xt);
            CALL(xt_to_body);
            ip = (XTPTR) POP_CELL();
	} else {
	    // a primitive, execute it
	    (*prim)(xt);
	}
    }
;C

\ Run the virtual machine interpretation loop	
C: (:) docolon ( -- )
    XT xt;
	
    do {
	// grab the next instruction
	xt = (*((XTPTR) ip++));
	
	// EXECUTE it
	PUSH_CELL(xt);
	execute(xt);
    } while(1);
;C


\ ---------- Low-level cross-compilation helpers ----------

<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS

\ Compile the compilation token for a given execution token. In
\ an indirect threaded interpreter this is the xt itself
: ITIL-CTCOMPILE, \ ( xt -- )
    XTCOMPILE, ;
' ITIL-CTCOMPILE, IS CTCOMPILE,

\ Compile the behaviour for NEXT, which is a literal 0
: ITIL-NEXT,
    0 COMPILE, ;
' ITIL-NEXT, IS NEXT,

WORDLISTS>


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
    PUSH_CELL(ip);
    CALL(calign);
    ip = POP_CELL();
;C


\ ---------- Control primitives ----------

\ Using the branch and jump words we can abstrcat how jumps are represented.

\ Jump unconditionally to the address compiled in the next cell
C: (BRANCH) branch ( -- )
  CELL offset = (CELL) *ip;
  ip = (XTPTR) (((BYTEPTR) ip) + offset);
;C

\ Test the top of the stack and either continue (if true) or
\ jump to the address compiled in the next cell (if false)
C: (?BRANCH) ( f -- )
  if(f)
    ip++;
  else
    CALL(branch);
;C

\ Jump calculations in CROSS for use in cross-compiling loops. If we
\ want to load control structures onto the target later, these also
\ need to be present there.
\ Shame about the repetition, but it's unavoidable in this case
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
\ Prepare for a forward jump
: JUMP-FORWARD ( -- a )
    TOP
    0 COMPILE, ;

\ Resolve a jump from the address on the stack to here
: JUMP-HERE ( a -- )
    TOP OVER -
    SWAP ! ;

\ Resolve a jump from here backwards to the address on the stack
: JUMP-BACKWARD ( a -- )
    TOP - 
    COMPILE, ;
WORDLISTS>


\ ---------- Common behaviours ----------
      
\ The run-time behaviour of a variable, returning its body address
C: (VAR) dovar ( -- addr )
    PUSH_CELL(_xt);
    CALL(xt_to_body);
    addr = POP_CELL();
;C

\ For a re-directable word, grab the indirect body address and jump to it,
\ pushing the real body address onto the stack first
\ sd: should we combine this with (VAR) and switch on redirectability?
C: (DOES) bracket_does ( -- body )
    CELLPTR iba;

    // put the body address on the stack first
    PUSH_CELL(_xt);
    CALL(xt_to_body);
    body = (CELLPTR) POP_CELL();

    // then jump to the IBA
    PUSH_CELL(_xt);
    CALL(xt_to_iba);
    iba = (CELLPTR) POP_CELL();
    PUSH_RETURN(ip);
    ip = (XT) *iba;
;C


\ ---------- (Re)-starting the interpreter ----------
\ sd: these need to be refactored, as they're not appropriate for non-interactive installations

CHEADER:

CELLPTR
user_variable( int n ) {
    XT _xt;

    PUSH_CELL(n);
    CALL(uservar);
    return (CELLPTR) POP_CELL();
}
;CHEADER

\ Reset the interpreter
C: (RESET) bracket_reset ( -- )
    // reset the stacks      
    DATA_STACK_RESET();
    RETURN_STACK_RESET();  

    // reset user variables
    *(user_variable(USER_STATE)) = (CELL) INTERPRETATION_STATE;    // interpretation state
    *(user_variable(USER_BASE)) = (CELL) 10;                       // decimal by convention
    *(user_variable(USER_TRACE)) = (CELL) 0;                       // not debugging
    CALL(reset_io);
    *(user_variable(USER__IN)) = -1;                               // line input in need of a refill
;C


\ Warm-start the interpreter
C: WARM warm_start ( -- )
    setjmp(env);

    // reset the interpreter
    CALL(bracket_reset);	
	
    // point the ip at the re-start vector and return
    PUSH_CELL(*(user_variable(USER__START_)));
    CALL(xt_to_body);
    ip = (XTPTR) POP_CELL();

    #ifdef DEBUGGING
        indent = 0;
    #endif
;C

\ Exit the interpreter
C: BYE ( -- )
    exit(0);
;C
