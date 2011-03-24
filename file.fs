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

\ User-level file management functions
\
\ The SAVE-INPUT and RESTORE-INPUT functions let programs
\ transparently save and restore the current input file
\ and position.
\
\ <TO .. TO> and <FROM ... FROM> re-direct the
\ virtual machine's standard input and output respectively,
\ and can be used to replace the standard streams with
\ files for a period.

\ ---------- Save/restore ----------

\ Save the current input state
: SAVE-INPUT \ ( -- fh pos 2 )
    INPUTSOURCE @
    DUP FILE-POSITION DROP
    2 ;

\ Restore the previously-save input state, if possible
: RESTORE-INPUT \ ( fh pos 2 -- f )
    DROP
    OVER REPOSITION-FILE ?DUP IF
	NIP
    ELSE
	INPUTSOURCE !
	EMPTY-TIB       \ force a refill at the next read attempt
	TRUE
    THEN ;


\ ---------- Re-directing I/O ----------

\ Re-direct subsequent output to the given file
: (<TO) \ ( fh -- ofh )
    OUTPUTSINK @
    SWAP OUTPUTSINK ! ;
: <TO \ ( "name" -- ofh )
    PARSE-WORD 2DUP W/O CREATE-FILE IF
	DROP
	TYPE 32 EMIT S" cannot be re-directed to" ABORT
    ELSE
	ROT 2DROP (<TO)
    THEN ;

\ Restore previous output stream
: TO> \ ( ofh -- ) 
    OUTPUTSINK @ CLOSE-FILE DROP
    OUTPUTSINK ! ;

\ Re-direct subsequent input from the given file
: (<FROM) \ ( fh -- ofh )
    INPUTSOURCE @
    SWAP INPUTSOURCE !
    EMPTY-TIB ;         \ force a refill when we try to read
: <FROM \ ( "name" -- ofh )
    PARSE-WORD 2DUP R/O OPEN-FILE IF
	DROP
	TYPE 32 EMIT S" cannot be re-directed from" ABORT
    ELSE
	ROT 2DROP (<FROM)
    THEN ;

\ Restore previous output stream
: FROM> \ ( ofh -- ) 
    INPUTSOURCE @ CLOSE-FILE DROP
    INPUTSOURCE ! EMPTY-TIB ;

