\ $Id$

\ File generators

<WORDLISTS ALSO CODE-GENERATOR ALSO DEFINITIONS

\ Generate a VM description from the given file
: GENERATE-VM \ ( addr n -- )
    >R >R
    <WORDLISTS ONLY FORTH ALSO CODE-GENERATOR
    R> R>
    INCLUDED
    WORDLISTS> ;

\ Generate the image, complete with primitives
: GENERATE-IMAGE \ ( -- )
    ." // " TIMESTAMP CR CR
    \ ." #include " QUOTES ." vm.h" QUOTES CR CR 

    S" vm.fs" GENERATE-VM
    S" x-arch-gcc-host.fs" GENERATE-VM CR
    
    GENERATE-CBLOCKS
    GENERATE-PRIMITIVES
    
    EMIT-IMAGE ;

WORDLISTS>

\ Save the image to the given file
: SAVE-IMAGE-TO \ ( "name" -- )
    PARSE-WORD 2DUP W/O CREATE-FILE IF
	DROP
	TYPE SPACE S" cannot be opened" ABORT
    ELSE
	ROT 2DROP
	(<TO)
	[CODE-GENERATOR] GENERATE-IMAGE
	TO>
    THEN ;
