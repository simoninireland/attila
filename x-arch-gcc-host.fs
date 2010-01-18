\ $Id$

\ Architecture definition for the host gcc compiler
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


    
