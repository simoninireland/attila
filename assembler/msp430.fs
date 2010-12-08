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

\ An assembler for the TI MSP430 series microcontroller
\
\ See http://mspgcc.sourceforge.net/manual/x223.html etc for details
\ of the instruction set


\ ---------- Binary and bit-field helper words ----------

\ Turn n into a w-bit bit field
: BF ( n w -- n' )
    1 SWAP LSHIFT 1-
    AND ;

\ Treat n as a w-bit number, and store it in a w-bit-wide field
\ in addr starting at bit sb, leaving all other bits untouched
: BF! ( n w sb addr -- )
    2OVER BF
    2SWAP SWAP - 1+
    LSHIFT
    OVER @ OR SWAP !
    DROP ;

\ Parse the next word as a binary number, placing it in the stack or
\ compiling it as a literal depending on the compilation state
: B ( "binary" -- n )
    PARSE-WORD 2DUP
    BINARY NUMBER? DECIMAL
    IF
	ROT 2DROP
    ELSE
	TYPE SPACE S" is not a valid binary number" ABORT
    THEN
    INTERPRETING? NOT IF
	POSTPONE LITERAL
    THEN ; IMMEDIATE

\ Print a 16-bit binary number, including leading 0 bits, without
\ changing the number base
: .BIN16 ( n -- )
    BASE @
    BINARY
    SWAP <#
    16 0 DO
	#
    LOOP
    #> TYPE SPACE
    BASE ! ;


\ ---------- Forming instructions ----------

\ Instruction fields
VARIABLE OPCODE-BITS
VARIABLE WIDTH-BITS
VARIABLE SOURCE-ADDRESSING-BITS
VARIABLE DESTINATION-ADDRESSING-BITS
VARIABLE SOURCE-BITS
VARIABLE DESTINATION-BITS
VARIABLE CONDITION-BITS
VARIABLE PC-OFFSET-BITS
VARIABLE INSTRUCTION-BITS

\ Stores and fetches
: OPCODE! OPCODE-BITS ! ;
: OPCODE@ OPCODE-BITS @ ;
: WIDTH! WIDTH-BITS ! ;
: WIDTH@ WIDTH-BITS @ ;
: SOURCE-ADDRESSING! SOURCE-ADDRESSING-BITS ! ;
: SOURCE-ADDRESSING@ SOURCE-ADDRESSING-BITS @ ;
: DESTINATION-ADDRESSING! DESTINATION-ADDRESSING-BITS ! ;
: DESTINATION-ADDRESSING@ DESTINATION-ADDRESSING-BITS @ ;
: SOURCE! SOURCE-BITS ! ;
: SOURCE@ SOURCE-BITS @ ;
: DESTINATION! DESTINATION-BITS ! ;
: DESTINATION@ DESTINATION-BITS @ ;
: CONDITION! CONDITION-BITS ! ;
: CONDITION@ CONDITION-BITS @ ;
: PC-OFFSET! PC-OFFSET-BITS ! ;
: PC-OFFSET@ PC-OFFSET-BITS @ ;

\ Instruction creation
: NEW-INSTRUCTION 0 INSTRUCTION-BITS ! ;
: INSTRUCTION-FIELD! ( n w sb -- )
    INSTRUCTION-BITS BF! ;

\ One-operand instructions pattern
: ONE-OPERAND-INSTRUCTION ( -- )
    NEW-INSTRUCTION
    B 000100                 6 15 INSTRUCTION-FIELD!
    OPCODE@                  3  9 INSTRUCTION-FIELD!
    WIDTH@                   1  6 INSTRUCTION-FIELD!
    DESTINATION-ADDRESSING@  2  5 INSTRUCTION-FIELD!
    DESTINATION@             4  3 INSTRUCTION-FIELD!
    INSTRUCTION-BITS @ ;

\ Two-operand instructions pattern
: TWO-OPERAND-INSTRUCTION ( -- )
    NEW-INSTRUCTION
    OPCODE@                  4 15 INSTRUCTION-FIELD!
    SOURCE@                  4 11 INSTRUCTION-FIELD!
    DESTINATION-ADDRESSING@  1  7 INSTRUCTION-FIELD!
    WIDTH@                   1  6 INSTRUCTION-FIELD!
    SOURCE-ADDRESSING@       2  5 INSTRUCTION-FIELD!
    DESTINATION@             4  3 INSTRUCTION-FIELD!
    INSTRUCTION-BITS @ ;

\ PC-relative jumpinstructions pattern
: JUMP-INSTRUCTION ( -- )
    NEW-INSTRUCTION
    B 001                    3 15 INSTRUCTION-FIELD!
    CONDITION@               3 12 INSTRUCTION-FIELD!
    PC-OFFSET@              10  9 INSTRUCTION-FIELD!
    INSTRUCTION-BITS @ ;


\ ---------- Forming registers and addressing modes ----------

\ Create a direct register accessor
: (REGISTER-DIRECT) ( r addr n -- )
    (CREATE) ,
  DOES>
    B 00 SWAP @ ;

\ Create an indirect register accessor
: (REGISTER-INDIRECT) ( r addr n -- )
    CLEAR-SCRATCH [CHAR] @ HOLD SHOLD SCRATCH> (CREATE) ,
  DOES>
    B 10 SWAP @ ;

\ Create an indirect register with post-increment accessor
: (REGISTER-INDIRECT-POSTINCREMENT) ( r addr n -- )
    CLEAR-SCRATCH [CHAR] @ HOLD SHOLD [CHAR] + HOLD SCRATCH> (CREATE) ,
  DOES>
    B 11 SWAP @ ;

\ Duplicate the top three elements -- 1 1/2 doubles :-)
: 3/2DUP ( a b c -- a b c a b c )
    2 PICK 2 PICK 2 PICK ;

\ Create all register accessors and addressing modes
: CREATE-REGISTER ( n "reg" -- )
    PARSE-WORD
    3/2DUP (REGISTER-DIRECT)
    \ 3/2DUP (REGISTER-INDEXED)
    3/2DUP (REGISTER-INDIRECT)
    (REGISTER-INDIRECT-POSTINCREMENT) ;
    
     
\ ---------- Registers, modifiers ----------

\ Widths
1 CONSTANT .B  \ byte
0 CONSTANT .W  \ 16-bit word

\ Registers and addressing
 0 CREATE-REGISTER R0
 1 CREATE-REGISTER R1
 2 CREATE-REGISTER R2
 3 CREATE-REGISTER R3
 4 CREATE-REGISTER R4
 5 CREATE-REGISTER R5
 6 CREATE-REGISTER R6
 7 CREATE-REGISTER R7
 8 CREATE-REGISTER R8
 9 CREATE-REGISTER R9
10 CREATE-REGISTER R10
11 CREATE-REGISTER R11
12 CREATE-REGISTER R12
13 CREATE-REGISTER R13
14 CREATE-REGISTER R14
15 CREATE-REGISTER R15

\ Register aliases
0 CREATE-REGISTER PC     \ program counter
1 CREATE-REGISTER SP     \ stack pointer
2 CREATE-REGISTER SR     \ status register
3 CREATE-REGISTER ZR     \ zero register


\ ---------- The instructions ----------

\ Rotate right through carry
: RRC ( da d -- )
    B 000 OPCODE!
    .B WIDTH!
    DESTINATION!
    DESTINATION-ADDRESSING!
    ONE-OPERAND-INSTRUCTION ;

\ Jump if last operation not zero
: JNZ ( offset -- )
    B 000 CONDITION!
    PC-OFFSET!
    JUMP-INSTRUCTION ;

\ Move src to dest, byte or word width
: MOV ( sa s da d w -- )
    B 0100 OPCODE!
    WIDTH!
    DESTINATION!
    DESTINATION-ADDRESSING!
    SOURCE!
    SOURCE-ADDRESSING!
    TWO-OPERAND-INSTRUCTION ;

    