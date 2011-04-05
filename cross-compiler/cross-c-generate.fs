\ $Id$

\ This file is part of Attila, a retargetable threaded interpreter
\ Copyright (c) 2007--2011, Simon Dobson <simon.dobson@computer.org>.
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

\ C code generator

\ ---------- Emitting cells ----------

\ sd: We use "normal" printing here because the endianness of the C compiler
\ and the image match.

\ Emit a cell as data: argument is a cell value
: C-EMIT-CELL \ ( t -- )
    ." (CELL) "
    DUP 0< IF
	." -" ABS
    THEN
    ." 0x" .HEX ;
    
\ Emit a cell as an address within the image: argument is a taddr
: C-EMIT-ADDRESS \ ( taddr -- )
    /CELL /MOD                           \ aligned cell
    ." (CELL) &image[0x" .HEX ." ]"
    ?DUP 0<> IF                          \ byte within cell
	SPACE ." + " .
    THEN ;

\ Emit a cfa as an address: argument is a counted string address
: C-EMIT-CFA \ ( caddr -- )
    ." (CELL) &"
    COUNT TYPE ;

\ resolve words for use in the image
' C-EMIT-CELL    IS EMIT-CELL
' C-EMIT-ADDRESS IS EMIT-ADDRESS
' C-EMIT-CFA     IS EMIT-CFA


\ ---------- Emitting the image ----------

\ Emit the image as a C data structure
: EMIT-IMAGE \ ( -- )
    ." CELL image[0x" IMAGE-SIZE .HEX ." ] = {" CR
    IMAGECELLS 0 DO
	." /* 0x" I .HEX SPACE ." */ "  
	I /CELL * DUP  @
	SWAP          E@ EXECUTE ." ," CR
    LOOP
    ." };" CR ;

\ Emit a small part of the image around the given target address
: EMIT-AROUND \ ( taddr -- )
    11 0 DO
	I 5 - 2DUP CELLS + DUP 0< IF
	    2DROP
	ELSE
	    SWAP 0= IF
		[FORTH] [CHAR] > EMIT
	    ELSE
		SPACE
	    THEN
	    DUP /CELL / .HEX SPACE
	    DUP TOP < IF 
		DUP @
		SWAP E@ EXECUTE
	    THEN
	    CR
	THEN
    LOOP
    DROP ;


\ ---------- File generators ----------

\ Generate a VM description from the given file
: GENERATE-VM \ ( addr n -- )
    >R >R
    <WORDLISTS ONLY FORTH ALSO CODE-GENERATOR
    R> R>
    INCLUDED
    WORDLISTS> ;

\ Generate the image from a template file
: GENERATE-IMAGE \ ( -- )
    ." // " TIMESTAMP CR CR

    ." // VM definition" CR
    CROSS-COMPILER-TARGET-VM-FILE GENERATE-VM CR CR

    ." // Runtime" CR
    ." #include " QUOTES CROSS-COMPILER-TARGET-RUNTIME-FILE TYPE QUOTES CR CR

    ." // Primitives table" CR
    GENERATE-PRIMITIVE-DECLARATIONS CR CR

    ." // C support" CR
    GENERATE-CBLOCKS CR CR

    ." // Primitives" CR
    GENERATE-PRIMITIVES CR CR

    ." // Initial image" CR
    EMIT-IMAGE CR CR ;

\ Summarise the image
: B>KB ( nb -- nkb )
    1024 /MOD SWAP IF 1+ THEN ;
: SUMMARISE-IMAGE ( -- )
    ." Image name: " CROSS-COMPILER-TARGET-NAME TYPE SPACE ." (" CROSS-COMPILER-OUTPUT-FILE TYPE ." )" CR
    ." VM characteristics: " [CROSS] /CELL . SPACE ." bytes/cell, "
                             [CROSS] BIGENDIAN? IF ." big-endian" ELSE ." little-endian" THEN CR
    20 SPACES                [CROSS] /CHAR . SPACE ." bytes/char" CR
    20 SPACES                [CROSS] DATA-STACK-SIZE . ." -cell data stack" CR
    20 SPACES                [CROSS] RETURN-STACK-SIZE . ." -cell return stack" CR
    20 SPACES                [CROSS] USER-SIZE . SPACE ." user variables" CR
    20 SPACES                [CROSS] TIB-SIZE . ." -byte terminal input buffer" CR
    ." Image size: " IMAGE-SIZE . SPACE ." cells, " IMAGE-SIZE [CROSS] /CELL * B>KB . ." Kb" CR  
    ." Initial dictionary size: " IMAGECELLS . SPACE ." cells, " IMAGECELLS [CROSS] /CELL * B>KB . SPACE ." Kb" CR
    ." Primitives: " PRIMITIVES LIST-LENGTH . CR ;
    
\ Save the image to the given file
: SAVE-IMAGE ( addr n -- )
    2DUP W/O CREATE-FILE IF
	DROP
	TYPE SPACE S" cannot be opened" ABORT
    ELSE
	ROT 2DROP
	(<TO)
	GENERATE-IMAGE
	TO>
    THEN
    CR SUMMARISE-IMAGE ;
