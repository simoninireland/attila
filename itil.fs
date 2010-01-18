\ $Id$

\ An indirect-threaded inner interpreter
\
\ This code is independent of memory model except that it
\ expects three primitives to be defined:
\
\  - xt_to_body  >BODY
\  - xt_to_cfa   >CFA
\  - xt_to_iba   >IBA

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


\ ---------- The inner interpreter ---------- 

\ Execute an xt from the stack
C: EXECUTE execute ( xt -- )
    PRIMITIVE prim;

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
            ip = (XTPTR) xt_to_body((XT) xt);
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


\ ---------- Common behaviours ----------
      
\ The run-time behaviour of a variable, returning its body address
C: (VAR) dovar ( -- addr )
    addr = (CELL) xt_to_body(_xt);
;C

\ For a re-directable word, grab the indirect body address and jump to it,
\ pushing the real body address onto the stack first
\ sd: should we combine this with (VAR) and switch on redirectability?
C: (DOES) bracket_does ( -- body )
    CELLPTR iba;

    xt_to_iba(_xt);
    iba = (CELLPTR) POP_CELL();
    PUSH_RETURN(ip);
    ip = (XT) *iba;
;C


\ ---------- Portable address arithmetic ----------
	
\ Compute a jump offset from a to TOP, in bytes
: JUMP> \ ( a -- offset )
    [CROSS] TOP SWAP - ;

\ Compute a jump offset from TOP to a, in bytes
: >JUMP \ ( a -- offset )
    [CROSS] TOP - ;

\ We also want the same code in CROSS
<WORDLISTS ONLY FORTH ALSO CROSS ALSO DEFINITIONS
: JUMP> \ ( a -- offset )
    [CROSS] TOP SWAP - ;
: >JUMP \ ( a -- offset )
    [CROSS] TOP - ;
WORDLISTS>


\ ---------- (Re)-starting the interpreter ----------

CHEADER:
PRIMITIVE uservar;

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
    ip = (XTPTR) xt_to_body((XT) *(user_variable(USER_EXECUTIVE)));
;C

\ Cold-start (same as WARM, for the moment)
C: COLD cold_start ( -- ) 
    CALL(warm_start);
;C
