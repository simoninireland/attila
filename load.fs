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

\ ---------- Loading sourcefiles ----------

\ Load a file
: (LOAD) \ ( fh -- )
    (<FROM)
    EXECUTIVE @ EXECUTE
    FROM> ;

\ Load a file
: LOAD \ ( addr len -- )
    2DUP R/O OPEN-FILE IF
	DROP TYPE 32 EMIT S" can't be loaded" ABORT
    ELSE
	ROT 2DROP
	(LOAD)
    THEN ;

    
\ ---------- Including files ----------
\ sd: no include path yet

\ Include a file, making use of the include path
: INCLUDED \ ( addr len -- )
    LOAD ;

\ Load the next file from the input stream
: INCLUDE \ ( "filename" -- )
    PARSE-WORD INCLUDED ;
