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

\ Records
\
\ Records are simply blocks of memory with accessor words returning the
\ address for each field. Unlike other implemnetations we just provide
\ address-based accessors, not type-based storage and retrieval words
\ (although that might be an option for the future).
\
\ Record types are created by, for example:
\
\ RECORD: PERSON
\    1 CELLS  FIELD: PERSON-IDENTIFIER
\    50 CHARS FIELD: PERSON-NAME
\ ;RECORD
\
\ Instances are created and accessed by, for example:
\
\ PERSON PERSON1
\ 1234 PERSON1 PERSON-IDENTIFIER !
\ S" Joe Bloggs" PERSON1 PERSON-NAME SMOVE
\ PERSON1 PERSON-NAME COUNT TYPE
\
\ Record types can be extended, adding extra filds to an existing type, using:
\
\ RECORD: EMPLOYEE EXTENDS: PERSON
\     1 CELLS FIELD: EMPLOYEE-NUMBER
\ ;RECORD
\
\ where any instance of EMPLOYEE also has all the fields of PERSON.
\
\ All fields are aligned on cell boundaries.


\ Return the size of a record type in bytes from its xt
: RECORDTYPESIZE ( xt -- bs )
    >BODY @ ;

\ Return the size in bytes of the record
: RECORDSIZE ( rec -- bs )
    @ ;

\ Create a record from a name and the xt of its type. This is functionally the behaviour
\ performed by the record type word, but it can be called like this to create
\ records for run-time-determined types
: (CREATE-RECORD) ( rec addr n -- )
    (DATA)                \ create the record
    RECORDTYPESIZE DUP ,  \ record the size...
    1 CELLS - ALLOT ;     \ ...and allocate the space needed

\ Basic record type creator
: (RECORD:) ( off addr n -- rectype off )
    (CREATE)
    TOP 0 ,           \ placeholder for size
    SWAP 1 CELLS +    \ initial offset within record, including space for size field
  DOES> ( "name" -- )
    DATA              \ create the record
    @ DUP ,           \ record the size...
    1 CELLS - ALLOT ; \ ...and allocate the space needed

\ Create a new record type with the given name
: RECORD: ( "name" -- rectype off )
    0 PARSE-WORD (RECORD:) ;

\ Mark the record type as extending another, i.e. having at least the same fields
: EXTENDS: ( off "base" -- off1 )
    ' RECORDTYPESIZE 1 CELLS -    \ offset is size of base record type minus its size field
    + ;

\ Finalise the record type structure
: ;RECORD ( rectype off -- )
    SWAP ! ;

\ Define a field with a size of bs bytes
: FIELD: ( off bs "name" -- off1 )
    CREATE
    OVER ,
    + ALIGN
  DOES> ( rec f -- addr )
    @ + ;

\ Copy a record to given address
: RECORDMOVE ( rec addr -- )
    OVER RECORDSIZE MOVE ;
