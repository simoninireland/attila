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

\ Common parts of word headers, for indirected threading.
\
\ The word header layout is as follows:
\
\           name       namelen bytes (CALIGNED, named model only)
\           namelen    1 byte        (CALIGNED, named model only)
\           status     1 byte
\    lfa -> link       1 cell
\     xt -> code       1 cell        (CALIGNED)
\  [ iba -> altbody    1 cell ]      (REDIRECTABLE words only)
\   body -> body       n bytes
\
\ Words are chained together and searched linearly.

\ ---------- Header access ----------

\ Convert an xt to the address of its code field (no-op in this model)
: >CFA ( xt -- xt ) ;

\ Manipulate the code field of a word
: CFA@ \ ( xt -- cf )
    >CFA @ ;
: CFA! \ ( cf xt -- )
    >CFA ! ;

\ Convert an xt to a link pointer containing the xt of the next word in the
\ definition chain
: >LFA \ ( xt -- lfa )
    /CELL - ;

\ Convert xt to its status field. The namelen and status are adjacent and
\ CALIGNED
: >STATUS \ ( xt -- addr )
    /CELL 2 * - 1+ ;

\ Convert xt to indirect body (DOES> behaviour) if present
\ (We don't check whether there actually *is* an IBA)
: >IBA \ ( xt -- iba )
    /CELL + ;

\ Manipulate the IBA of a word
: IBA@ \ ( xt -- iba )
    >IBA @ ;
: IBA! \ ( addr xt -- )
    >IBA ! ;


\ ---------- Status and body management ----------

\ Mask-in the given mask to the status of a word
: SET-STATUS \ ( f xt -- )
    >STATUS DUP C@
    ROT OR
    SWAP C! ;

\ Mask-out the given mask to the status of a word
: CLEAR-STATUS \ ( f xt -- )
    >STATUS DUP C@
    ROT INVERT AND
    SWAP C! ;

\ Get the status of the given word
: GET-STATUS \ ( xt -- s )
    >STATUS C@ ;

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

\ Convert xt to body address, accounting for iba if present
: >BODY \ ( xt -- addr )
    DUP >IBA
    SWAP REDIRECTABLE? IF
	/CELL +
    THEN ;

