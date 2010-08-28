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
\ The model has to export six routines as named primitives, that then
\ get called from within the inner interpreter:
\
\  - xt_to_body   >BODY
\  - xt_to_cfa    >CFA
\  - xt_to_iba    >IBA
\  - xt_to_status >STATUS
\  - xt_to_name   >NAME
\  - xt_to_lfa    >LFA


\ ---------- Memory management ----------

\ TOP and HERE are the same in this model
: TOP  2 USERVAR ( TOP ) @ ;
: HERE TOP ;
: LASTXT 10 USERVAR ( LAST ) @ ;

\ Compile a character
: CCOMPILE, \ ( c -- )
    TOP C!
    1 2 USERVAR ( TOP ) +! ;

\ Align code address on cell boundaries
\ : CALIGN \ ( addr -- aaddr )
\     DUP /CELL MOD ?DUP 0<> IF /CELL SWAP - + THEN ;
C: CALIGN calign ( addr -- aaddr )
  aaddr = addr;
  if((aaddr % sizeof(CELL)) > 0)
    aaddr = aaddr + (sizeof(CELL) - (aaddr % sizeof(CELL)));
;C
: CALIGNED \ ( -- )
    TOP CALIGN TOP - ?DUP IF
	0 DO
	    0 CCOMPILE,
	LOOP
    THEN ;

\ Compilation primitives for cells, real addresses, strings, addresses, xts
\ sd: should be refactored to use [IF] etc
: COMPILE, \ ( n -- )
    CALIGNED
    /CELL 0 DO
	BIGENDIAN? IF
	    DUP /CELL I - 1- 8 * RSHIFT 255 AND CCOMPILE,
	ELSE
	    DUP 255 AND CCOMPILE,
	    8 RSHIFT
	THEN
    LOOP DROP ;
: SCOMPILE, \ ( addr n -- )
    DUP CCOMPILE,
    0 DO
	DUP C@ CCOMPILE,
	1+
    LOOP
    DROP ;

\ There's no distinction between addresses at run-time
: RACOMPILE,  COMPILE, ;
: ACOMPILE,   COMPILE, ;
: XTCOMPILE,  ACOMPILE, ;
: CFACOMPILE, COMPILE, ;

\ CTCOMPILE, is defined in the inner interpreter

\ In this model, data and code compilation are the same
: C,      CCOMPILE, ;
: ,       COMPILE, ;
: RA,     RACOMPILE, ;
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

\ Memory access also doesn't distinguish between address types
\ !, @, C@, C!, 2@ amd 2! are in core.fs: these words are the "manipulators"
\ for architectures that need to handle the different types differently
: A!   ! ;          \ Attila addresses
: A@   @ ;
: RA!  ! ;          \ "real" (hardware) addresses
: RA@  @ ;
: XT!  ! ;          \ xts
: XT@  @ ;
\ See also CFA! and CFA@, IBA! and IBA@ below, which manipulate word headers


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
\ : >LFA \ ( xt -- lfa )
\     1 CELLS - ;
C: >LFA xt_to_lfa ( xt -- lfa )
  lfa = (CELL) ((BYTEPTR) xt - sizeof(CELL));
;C

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
\ : >STATUS \ ( xt -- addr )
\     2 CELLS - 1+ ;
C: >STATUS xt_to_status ( xt -- addr )
  addr = (CELL) ((BYTEPTR) xt - 2 * sizeof(CELL) + 1);
;C

\ Convert an xt to a name string. addr will be CALIGNED
\ : >NAME \ ( xt -- addr namelen )
\     >LFA 1 CELLS - DUP C@
\     DUP >R
\     -
\     DUP /CELL MOD 0<> IF
\         /CELL -
\     THEN CALIGN                \ ensure we're aligned on the previous cell boundary
\     R> ;
C: >NAME xt_to_name ( -- addr n )
  BYTEPTR a;

  CALL(xt_to_lfa);
  a = (BYTEPTR) POP_CELL();
  a -= sizeof(CELL);
  n = (CELL) *a;
  a -= n;
  if((n % sizeof(CELL)) > 0)
    a -= sizeof(CELL);
  PUSH_CELL(a);
  CALL(calign);
  addr = POP_CELL();
;C

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
    SWAP C! ;

\ Mask-out the given mask to the status of a word
: CLEAR-STATUS \ ( f xt -- )
    >STATUS DUP C@
    -ROT INVERT AND
    SWAP C! ;

\ Get the status of the given word
: GET-STATUS \ ( xt -- s )
    >STATUS C@ ;

\ Make the last word defined IMMEDIATE
: IMMEDIATE \ ( -- )
    1 ( IMMEDIATE-MASK ) LASTXT SET-STATUS ;

\ Test whether the given word is IMMEDIATE
: IMMEDIATE? \ ( xt -- f )
    GET-STATUS 1 ( IMMEDIATE-MASK ) AND 0<> ; 

\ Make the last word defined REDIRECTABLE
: REDIRECTABLE \ ( -- )
    2 ( REDIRECTABLE-MASK ) LASTXT SET-STATUS ; 

\ Test whether the given word is REDIRECTABLE
: REDIRECTABLE? \ ( xt -- f )
    GET-STATUS 2 ( REDIRECTABLE-MASK ) AND 0<> ; 

\ Convert xt to body address, accounting for iba if present. This
\ has to be coded as a primitive called xt_to_body, which is used
\ by the inner interpreter, and it *has* to match the other
\ definitions (>STATUS, REDIRECTABLE-MASK, IMMEDIATE?)  even though
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
  addr = xt + sizeof(CELL);
  if(status & REDIRECTABLE_MASK)
    addr += sizeof(CELL);
;C


\ ---------- Header creation ----------

\ Create a header for the named word with the given code field. We must handle the
\ name being of zero length for :NONAME definitions
: (HEADER,) \ ( addr n cf -- xt )
    CALIGNED                 \ align TOP on the next cell boundary 
    >R                       \ stash the code field
    DUP >R                   \ ...and the name length
    ?DUP 0> IF               \ if the name is not null
	0 DO
	    DUP C@ CCOMPILE,
	    1+
	LOOP
    THEN
    DROP
    CALIGNED      
    R> CCOMPILE,             \ compile the name length
    0 CCOMPILE,              \ status byte
    CALIGNED
    LASTXT ACOMPILE,         \ compile the link pointer
    TOP                      \ the txt
    R> CFACOMPILE,           \ the code pointer
    DUP 10 USERVAR XT! ;     \ update LAST


\ ---------- Basic finding ----------
\ sd: Hiding is incredibly ugly but incredibly useful for fine-tuning

\ Hide/unhide a word from the finder. Since a hidden word can't be found,
\ you need to stash its xt if you want to be able ti unhide it later
: (HIDE) ( xt -- )
    4 ( HIDDEN-MASK ) SWAP SET-STATUS ;
: (UNHIDE) ( xt -- )
    4 ( HIDDEN-MASK ) SWAP CLEAR-STATUS ;

\ Test whether a word is hidden
: HIDDEN? ( xt -- f )
    GET-STATUS 4 ( HIDDEN-MASK ) AND 0<> ;


\ Find the named word in the word list starting from the given
\ xt. Return 0 if the word can't be found, or it's xt and either
\ 1 for normal or -1 for IMMEDIATE words
C: (FIND) bracket_find ( addr namelen x -- )
    CHARACTERPTR taddr;
    CELL tlen;
    XT xt;
    CELLPTR link;
    BYTEPTR status;
	
    xt = (XT) NULL;
    while(x != (BYTEPTR) NULL) {
        PUSH_CELL(x);
        CALL(xt_to_name);
        tlen = POP_CELL();
        taddr = POP_CELL();
        #ifdef DEBUGGING
            printf("-> %s\n", create_unix_string(taddr, tlen));
        #endif
        PUSH_CELL(x);
        CALL(xt_to_status);
        status = (BYTEPTR) POP_CELL();
        if((((*status) & HIDDEN_MASK) == 0) &&
           (namelen == tlen) &&
	   (strncasecmp(addr, taddr, namelen) == 0)) {
	    xt = x;   x = NULL;
	} else {
            PUSH_CELL(x);
            CALL(xt_to_lfa);
	    link = POP_CELL();
	    x = (XT) (XTPTR) (*link);
	}
    }

    PUSH_CELL(xt);
    if(xt != (XT) NULL) {
	PUSH_CELL(xt);
	CALL(xt_to_status);
	status = (BYTEPTR) POP_CELL();
        PUSH_CELL((*status & IMMEDIATE_MASK) ? -1 : 1);
    }
;C

	    