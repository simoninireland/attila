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

    ." // Primitives table" CR
    GENERATE-PRIMITIVE-DECLARATIONS CR CR

    ." // VM definition" CR
    S" vm.fs" GENERATE-VM CR CR

    ." // Architecture definition" CR
    ." #include " QUOTES ." x-arch-gcc-host.h" QUOTES CR CR 

    ." // C support" CR
    GENERATE-CBLOCKS CR CR

    ." // Primitives" CR
    GENERATE-PRIMITIVES CR CR

    ." // Image" CR
    EMIT-IMAGE CR CR ;

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
