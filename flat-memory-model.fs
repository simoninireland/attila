\ $Id$

\ A simple, flat memory model with headers, C version.
\
\ A "flat" memory model in which headers, code and data live
\ live in a single contiguous block.
\
\           name       namelen bytes (CALIGNED)
\           namelen    1 byte        (CALIGNED)
\           status     1 byte
\    lfa -> link       1 cell
\     xt -> code       1 cell        (CALIGNED)
\  [ iba -> altbody    1 cell ]      (REDIRECTABLE words only)
\   body -> body       n bytes
\
\ The model has to export five routines as named primitives, that then
\ get called from within the inner interpreter:
\
\  - xt_to_body   >BODY
\  - xt_to_cfa    >CFA
\  - xt_to_iba    >IBA
\  - xt_to_status >STATUS
\  - xt_to_name   >NAME


\ ---------- Memory management ----------

\ TOP and HERE are the same in this model
: TOP  2 USERVAR @ ; \ (TOP)
: HERE TOP ;
: LASTXT 9 USERVAR @ ; \ LAST

\ Compile a character
: CCOMPILE, \ ( c -- )
    TOP C!
    1 2 USERVAR +! ; \ (TOP)

\ Align code address on cell boundaries
: CALIGN \ ( addr -- aaddr )
    DUP /CELL MOD ?DUP 0<> IF /CELL SWAP - + THEN ;
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;

\ Compilation primitives for cells, real addresses, strings, addresses, xts
\ sd: currently hard-coded as being little-endian
: COMPILE, \ ( n -- )
    CALIGNED
    /CELL 0 DO
	DUP 255 AND CCOMPILE,
	8 RSHIFT
    LOOP DROP ;
: SCOMPILE, \ ( addr n -- )
    DUP CCOMPILE,
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    DROP ;

\ There's no distinction between addresses at run-time
: RCOMPILE,   COMPILE, ;
: ACOMPILE,   COMPILE, ;
: XTCOMPILE,  ACOMPILE, ;
: CFACOMPILE, COMPILE, ;

\ CTCOMPILE, is defined in the inner interpreter

\ In this model, data and code compilation are the same
: C,      COMPILE, ;
: ,       COMPILE, ;
: R,      RCOMPILE, ;
: A,      ACOMPILE, ;
: XT,     XTCOMPILE, ;
: S,      SCOMPILE, ;
: ALIGN   CALIGN ;
: ALIGNED CALIGNED ;

\ ALLOTting data space simply compiles zeros
: ALLOT ( n -- )
    0 DO
	0 C,
    LOOP ;


\ ---------- Header access ----------

\ Convert an xt to the address of its code field (no-op in this model)
\ : >CFA ; \ ( xt -- cfa )
C: >CFA xt_to_cfa ( xt -- xt )
;C

\ Manipulate the code field of a word
: CFA@ \ ( xt -- cf )
    >CFA @ ;
: CFA! \ ( cf xt -- )
    >CFA ! ;

\ Convert an xt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( xt -- lfa )
    1 CELLS - ;

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
: >STATUS \ ( xt -- addr )
    2 CELLS - 1+ ;

\ Convert an xt to a name string. addr will be CALIGNED
: >NAME \ ( xt -- addr namelen )
    >LFA 1 CELLS - DUP C@
    DUP >R
    - 1-
    1 CELLS - CALIGN        \ ensure we're aligned on the previous cell boundary
    R> ;

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
\ : >IBA \ ( xt -- iba )
\     1 CELLS + ;
C: >IBA xt_to_iba ( xt -- iba )
    iba = (CELL) ((BYTEPTR) xt + sizeof(CELL));
;C

\ Manipulate the IBA of a word
: IBA@ \ ( xt -- iba )
    >IBA @ ;
: IBA! \ ( addr xt -- )
    >IBA ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f xt -- )
    >STATUS DUP C@
    -ROT OR
    SWAP ! ;

\ Get the status of the given word
: GET-STATUS \ ( xt -- s )
    >STATUS @ ;

\ Status bit masks
: IMMEDIATE-MASK    1 ;
: REDIRECTABLE-MASK 2 ;

\ Make the last word defined IMMEDIATE
: IMMEDIATE \ ( -- )
    IMMEDIATE-MASK LASTXT SET-STATUS ;

\ Test whether the given word is IMMEDIATE
: IMMEDIATE? \ ( xt -- f )
    GET-STATUS IMMEDIATE-MASK AND 0<> ; 

\ Make the last word defined REDIRECTABLE
: REDIRECTABLE \ ( -- )
    REDIRECTABLE-MASK LASTXT SET-STATUS ; 

\ Test whether the given word is REDIRECTABLE
: REDIRECTABLE? \ ( xt -- f )
    GET-STATUS REDIRECTABLE-MASK AND 0<> ; 

\ Convert xt to body address, accounting for iba if present. This
\ has to be coded as a primitive called xt_to_body, which is used
\ by the inner interpreter, and it *has* to match the other
\ definitions (>STATUS, IMEDIATE-MASK, IMMEDIATE?)  even though
\ it doesn't use them directly 
\ : >BODY \ ( xt -- addr )
\     DUP >IBA
\     SWAP REDIRECTABLE? IF
\ 	1 CELLS +
\     THEN ;
C: >BODY xt_to_body ( xt -- addr )
  BYTEPTR statusptr;
  BYTE status;

  statusptr = ((BYTEPTR) xt) - 2 * sizeof(CELL) + 1;
  status = *statusptr;
  addr = xt + 1;
  if(status & 1)
    addr += 1;
;C


\ ---------- Header creation ----------

\ Create a header for the named word with the given code field
: (HEADER,) \ ( addr n cf -- xt )
    CALIGNED         \ align TOP on the next cell boundary 
    >R               \ stash the code field
    DUP >R           \ compile the name
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    DROP
    CALIGNED      
    R> CCOMPILE,     \ compile the name length
    0 CCOMPILE,      \ status byte
    CALIGNED
    0 COMPILE,       \ compile an empty link pointer
    TOP              \ the txt
    R> CFACOMPILE, ; \ the code pointer


\ ---------- Basic finding ----------

\ Find the named word in the word list starting from the given
\ xt. Return 0 if the word can't be found, or it's xt and either
\ 1 for normal or -1 for IMMEDIATE words
C: (FIND) bracket_find ( addr namelen x -- )
    CHARACTERPTR taddr;
    CELL tlen;
    XT xt;
    CELLPTR link;
    BYTE status;
	
    xt = (XT) NULL;
    while(x != (BYTEPTR) NULL) {
        PUSH_CELL(x);
        xt_to_name(_xt);
        tlen = POP_CELL();
        taddr = POP_CELL();
	if((namelen == tlen) &&
	   (strncasecmp(addr, taddr, namelen) == 0)) {
	    xt = x;   x = NULL;
	} else {
            PUSH_CELL(x);
            xt_to_lfa(_xt);
	    link = POP_CELL();
	    x = (XT) (XTPTR) (*link);
	}
    }

    PUSH_CELL(xt);
    if(xt != (XT) NULL) {
	PUSH_CELL(xt);
	xt_to_status(_xt);
	status = POP_CELL();
        PUSH_CELL((status & IMMEDIATE_MASK) ? -1 : 1);
    }
;C
