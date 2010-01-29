\ $Id$

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
// #define ITIL_DEBUGGING 1

#ifdef ITIL_DEBUGGING
static int indent;
static char buf[256];
#endif
;CHEADER

\ ---------- The inner interpreter ---------- 

\ Execute an xt from the stack
C: EXECUTE execute ( xt -- )
    PRIMITIVE prim;

    #ifdef ITIL_DEBUGGING
        // print word
	int i;
	if(xt == (XT) NULL) { 
            indent -= 3;
        } else {
	    PUSH_CELL(xt);
	    CALL(xt_to_name);
	    CELL n = POP_CELL();
	    BYTEPTR addr = (BYTEPTR) POP_CELL();
	    strncpy(buf, addr, n);   buf[n] = '\0';
            for(i = 0; i < indent; i++) printf(" ");
	    printf("%x %s\n", (CELL) (((BYTEPTR) xt - (BYTEPTR) image) / 8), buf);
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
    CALL(calign)
    ip = POP_CELL();
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

\ Compute a jump offset from a to TOP, in bytes
: JUMP> \ ( a -- offset )
    2 USERVAR @ SWAP - ;
\ Compute a jump offset from TOP to a, in bytes
: >JUMP \ ( a -- offset )
    2 USERVAR @ - ;

\ We also want the same code in CROSS for use by control structures
<WORDLISTS ONLY FORTH ALSO CROSS DEFINITIONS
\ Compute a jump offset from a to TOP, in bytes
: JUMP> \ ( a -- offset )
    [CROSS] TOP SWAP - ;

\ Compute a jump offset from TOP to a, in bytes
: >JUMP \ ( a -- offset )
    ." >JUMP " dup .hex space [cross] top .hex
    [CROSS] TOP -  ;
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

    PUSH_CELL(_xt);
    CALL(xt_to_iba);
    iba = (CELLPTR) POP_CELL();
    PUSH_RETURN(ip);
    ip = (XT) *iba;
;C


\ ---------- (Re)-starting the interpreter ----------

CHEADER:

CELLPTR
user_variable( int n ) {
    XT _xt;

    PUSH_CELL(n);
    CALL(uservar);
    return (CELLPTR) POP_CELL();
}
;CHEADER

\ Warm-start the interpreter
C: WARM warm_start ( -- )
    setjmp(env);

    // reset the stacks      
    DATA_STACK_RESET();
    RETURN_STACK_RESET();  

    // reset user variables
    *(user_variable(USER_STATE)) = (CELL) INTERPRETATION_STATE;
    *(user_variable(USER_BASE)) = (CELL) 10;
    *(user_variable(USER_TRACE)) = (CELL) 0;
    *(user_variable(USER_INPUTSOURCE)) = (CELL) stdin;
    *(user_variable(USER__IN)) = -1;                // in need of a refill
	
    // point the ip at the executive and return
    PUSH_CELL(*(user_variable(USER_EXECUTIVE)));
    CALL(xt_to_body);
    ip = (XTPTR) POP_CELL();

    #ifdef ITIL_DEBUGGING
        indent = 0;
    #endif
;C

\ Cold-start (same as WARM, for the moment)
\ sd: COLD *must* be a primitive 
C: COLD cold_start ( -- ) 
    CALL(warm_start);
;C
