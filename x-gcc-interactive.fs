\ $Id$

\ Cross-compiler primitives for interactive gcc target


\ ---------- Stack management ----------

C:
CELLPTR image;
XTPTR ip;

CELLPTR data_stack, data_stack_base;
XTPTR return_stack, return_stack_base;

// ---------- Debugging ----------

#define DIE( msg ) (printf("DIE: %s\n", msg), exit(1), NULL)


;C

\ ---------- Memory management ----------

\ Store a cell into a given image address
PRIMITIVE: ! ( n addr -- )
    image[addr] = n;
;PRIMITIVE

\ Fetch a cell from a given image address
PRIMITIVE: @ ( addr -- n )
    n = image[addr];
;PRIMITIVE

\ Store a character into the given image address
PRIMITIVE: C! ( c addr -- )
    long cell;
    int offset;	
    BYTEPTR ptr;
	
    cell = addr / sizeof(CELL);    offset = addr % sizeof(CELL);
    ptr = (BYTEPTR) &image[cell];
    ptr[offset] = c;
;PRIMITIVE

\ Fetch a character from the given image address
PRIMITIVE: C@ ( addr -- c )
    long cell;
    int offset;	
    BYTEPTR ptr;
	
    cell = addr / sizeof(CELL);    offset = addr % sizeof(CELL);
    ptr = (BYTEPTR) &image[cell];
    c = ptr[offset];
;PRIMITIVE


\ ---------- Control primitives ----------

\ Jump unconditionally to the address compiled in the next cell
PRIMITIVE: (BRANCH) ( -- )
  CELL offset;

  offset = image[(int) ip];
  ip = (XTPTR) ((BYTEPTR) ip + offset);
;PRIMITIVE

\ Test the top of the stack and either continue (if true) or
\ jump to the address compiled in the next cell (if false)
PRIMITIVE: (?BRANCH) ( f -- )
  CELL offset;
	
  if(f)
    ip++;
  else {
    offset = image[(int) ip];
    ip = (XTPTR) ((BYTEPTR) ip + offset);
  }
;PRIMITIVE

	
\ ---------- Inner interpreter ----------

\ Execute an xt from the stack
PRIMITIVE: EXECUTE( xt -- )
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

\ Run the virtual machine interpretation loop
PRIMITIVE: (:) docolon ( -- ) " bracket colon"
    XT xt;
	
    do {
	// grab the next instruction
        xt = image[(int) ip++];

	// run the debug code if we're tracing
//	if(*(user_variable(USER_TRACE))) {
//            DEBUG(xt);
//	}
		
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
    ip = (XT) image[(int) iba];
;PRIMITIVE
