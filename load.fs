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

\ Source file inclusion

\ Load a file
: (LOAD) \ ( fh -- )
    (<FROM)
    EXECUTIVE @ EXECUTE
    FROM> ;

\ Test whether the given file exists and is readable
: READABLE? ( addr n -- f )
    R/O OPEN-FILE IF
	DROP FALSE
    ELSE
	CLOSE-FILE DROP
	TRUE
    THEN ;
    
\ Load a file
: LOAD \ ( addr len -- )
    2DUP R/O OPEN-FILE IF
	DROP TYPE 32 EMIT ( SPACE ) S" can't be loaded" ABORT
    ELSE
	ROT 2DROP
	(LOAD)
    THEN ;

