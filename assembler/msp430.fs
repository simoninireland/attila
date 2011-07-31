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

\ Instruction bits
VARIABLE INSTRUCTION-BITS

\ Instructions and offset compilation -- currently debugging mode
: INSTRUCTION, ( -- )
    INSTRUCTION-BITS @ .BIN16 SPACE ;
: OFFSET, ( off -- )
    . SPACE ;

\ Instruction creation
: NEW-INSTRUCTION 0 INSTRUCTION-BITS ! ;
: INSTRUCTION-FIELD! ( n w sb -- )
    INSTRUCTION-BITS BF! ;

\ One-operand instructions pattern
: ONE-OPERAND-INSTRUCTION ( doff da d w opcode -- )
    NEW-INSTRUCTION
    B 000100       6 15 INSTRUCTION-FIELD!
    ( opcode )     3  9 INSTRUCTION-FIELD!
    ( width )      1  6 INSTRUCTION-FIELD!
    ( d )          4  3 INSTRUCTION-FIELD!
    ( da ) DUP     2  5 INSTRUCTION-FIELD!
    INSTRUCTION,

    \ add any offset
    B 1 = IF
	OFFSET,
    ELSE
	DROP
    THEN ;

\ Two-operand instructions pattern
: TWO-OPERAND-INSTRUCTION ( soff sa s doff da d w opcode -- )
    NEW-INSTRUCTION
    ( opcode)         4 15 INSTRUCTION-FIELD!
    ( w )             1  6 INSTRUCTION-FIELD!
    ( d )             4  3 INSTRUCTION-FIELD!
    ( da ) DUP >R     1  7 INSTRUCTION-FIELD!
    ( doff ) 3 ROLL
    ( s )             4 11 INSTRUCTION-FIELD!
    ( sa ) DUP        2  5 INSTRUCTION-FIELD!
    INSTRUCTION,

    \ add any offsets
    B 01 = IF
	OFFSET,
    ELSE
	DROP
    THEN
    R> B 1 = IF
	OFFSET,
    ELSE
	DROP
    THEN ;

\ PC-relative jump instructions pattern
: JUMP-INSTRUCTION ( pcoff cond -- )
    NEW-INSTRUCTION
    B 001        3 15 INSTRUCTION-FIELD!
    ( cond )     3 12 INSTRUCTION-FIELD!
    ( pcoff )   10  9 INSTRUCTION-FIELD!
    INSTRUCTION, ;


\ ---------- Forming registers and addressing modes ----------

\ Create a direct register accessor, e.g. R1
\ Value taken from the register
: (REGISTER-DIRECT) ( r addr n -- )
    (CREATE) ,
  DOES> ( -- 0 a r )
    0 B 00 -ROT @ ;

\ Create a register indexed register accessor e.g. (+R1)
\ Value taken from the memory addressed by the value the register plus the offset
: (REGISTER-INDEXED) ( r addr n -- )
    CLEAR-SCRATCH [CHAR] ( HOLD [CHAR] + HOLD SHOLD [CHAR] ) HOLD SCRATCH> (CREATE) ,
  DOES> ( -- a r ) \ expecting an offset on the stack immediately before
    B 01 SWAP @ ;

\ Create an indirect register accessor, e.g. @R1
\ Value taken from the memory addresses by the value in the register
: (REGISTER-INDIRECT) ( r addr n -- )
    CLEAR-SCRATCH [CHAR] @ HOLD SHOLD SCRATCH> (CREATE) ,
  DOES>
    0 B 10 -ROT @ ;

\ Create an indirect register with post-increment accessor, e.g. @R1+
\ Value taken from the memory addressed by the value in the register,
\ with the register then being incremented by 1 (byte) or 2 (word)
: (REGISTER-INDIRECT-POSTINCREMENT) ( r addr n -- )
    CLEAR-SCRATCH [CHAR] @ HOLD SHOLD [CHAR] + HOLD SCRATCH> (CREATE) ,
  DOES> ( -- 0 a r )
    0 B 11 -ROT @ ;

\ Duplicate the top three elements -- 1 1/2 doubles :-)
: 3/2DUP ( a b c -- a b c a b c )
    2 PICK 2 PICK 2 PICK ;

\ Swap two 3/2 numbers
: 3/2SWAP ( a b c d e f -- d e f a b c )
    5 -ROLL 5 -ROLL 5 -ROLL ;

\ Create all register accessors and addressing modes
: CREATE-REGISTER ( n "reg" -- )
    PARSE-WORD
    3/2DUP (REGISTER-DIRECT)
    3/2DUP (REGISTER-INDEXED)
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

\ Status bits
0 CONSTANT C             \ carry
1 CONSTANT Z             \ zero
2 CONSTANT N             \ negative
3 CONSTANT GIE           \ interupt enable
4 CONSTANT CPUOFF
5 CONSTANT OSCOFF
6 CONSTANT SCG0
7 CONSTANT SCG1
8 CONSTANT V            \ overflow


\ ---------- The instructions ----------

\ Add src to dest
: ADD
    B 0101 TWO-OPERAND-INSTRUCTION ;

\ Add src to dest with carry
: ADDC
    B 0110 TWO-OPERAND-INSTRUCTION ;

\ AND src with dest
: AND
    B 1111 TWO-OPERAND-INSTRUCTION ;

\ Clear src's bit in dest
: BIC
    B 1100 TWO-OPERAND-INSTRUCTION ;

\ Set src's bit in dest
: BIS
    B 1101 TWO-OPERAND-INSTRUCTION ;

\ Test src's bit in dest, setting flags for comparison
: BIT
    B 1011 TWO-OPERAND-INSTRUCTION ;

\ Make a subroutine call, pushing PC
: CALL
    .W B 101 ONE-OPERAND-INSTRUCTION ;

\ Subtract src from dest and set flags for comparison 
: CMP
    B 1001 TWO-OPERAND-INSTRUCTION ; \ CHECK: wrong opcode?

\ Add src to dest with carry, binary-coded decimal
: DADD
    B 1010 TWO-OPERAND-INSTRUCTION ;

\ Jumps
: JNZ B 000 JUMP-INSTRUCTION ; \ not zero
: JNE B 000 JUMP-INSTRUCTION ; \ not equal
: JEQ B 001 JUMP-INSTRUCTION ; \ equal
: JZ  B 001 JUMP-INSTRUCTION ; \ zero
: JNC B 010 JUMP-INSTRUCTION ; \ no carry
: JLO B 010 JUMP-INSTRUCTION ; \ unsigned <
: JC  B 011 JUMP-INSTRUCTION ; \ carry
: JHS B 011 JUMP-INSTRUCTION ; \ unsigned >=
: JN  B 100 JUMP-INSTRUCTION ; \ negative
: JGE B 101 JUMP-INSTRUCTION ; \ signed >=
: JL  B 110 JUMP-INSTRUCTION ; \ signed <
: JMP B 111 JUMP-INSTRUCTION ; \ unconditionally

\ Move src to dest, byte or word width
: MOV
    B 0100 TWO-OPERAND-INSTRUCTION ;

\ Push operand onto the stack
: PUSH
    B 100 ONE-OPERAND-INSTRUCTION ;

\ Return from interrupt
: RETI
    R1 \ unused
    .W \ unused
    B 110 ONE-OPERAND-INSTRUCTION ;

\ 8-bit arithmetic right-shift
: RRA
    B 010 ONE-OPERAND-INSTRUCTION ;

\ Rotate right through carry
: RRC
    B 000 ONE-OPERAND-INSTRUCTION ;

\ Subtract src from dest 
: SUB
    B 1001 TWO-OPERAND-INSTRUCTION ;

\ Subtract src from dest and add carry 
: SUBC
    B 0111 TWO-OPERAND-INSTRUCTION ;

\ Swap 8-bit register halves
: SWP.B
    .B B 001 ONE-OPERAND-INSTRUCTION ;

\ Sign-extend 8 to 16 bits
: SXT.W
    .W B 011 ONE-OPERAND-INSTRUCTION ;

\ XOR src with dest
: XOR
    B 1110 TWO-OPERAND-INSTRUCTION ;


\ ---------- Emulated instructions ----------

\ No-op
: NOP R3 R3 .W MOV ;

\ Pop the stack
: POP @SP+ 3/2SWAP .W MOV ; 

\ Branch relative
: BR PC MOV ;

\ Return from subroutine
: RET @SP+ PC .W MOV ;

