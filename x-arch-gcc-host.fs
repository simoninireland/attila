\ $Id$

\ Architure definition for the host gcc compiler
\
\ This file is pre-processed to generate the necessary
\ definitions, typically in C. It should contain only
\ simple code such as CONSTANT declarations

\ The size of a cell in bytes
4 CONSTANT /CELL

\ Endianness of the processor:
\   0 = little-endian
\   1 = big-endian
0 CONSTANT BIGENDIAN?

\ Memory sizes, in cells
256 1024 * CONSTANT IMAGE-SIZE
100        CONSTANT DATA-STACK-SIZE
100        CONSTANT RETURN-STACK-SIZE
256        CONSTANT TIB-SIZE

\ Include files
C-INCLUDE x-types-gcc.h   \ type mapping
C-INCLUDE x-stack-gcc.h   \ stack implementation
