\ $Id$

\ The primitive cross-compiler
\
\ Primitives are stored in Attila source files alongide Attila code.
\ When loaded into the cross-compiler they are converted into
\ stylised C functions and output to a file ready for later compilation.
\
\ This code performs the same functions as tools/primgen does for
\ bootstrapping Attila.

\ ---------- Constants and data structures ----------

\ The largest primitive source code size we can handle
1024 VALUE LARGEST-PRIMITIVE


\ ---------- Primitive parser ----------



\ ---------- Primitive generation ----------

\ Generate the C source code for a primitive
: (GENERATE-PRIMITIVE) \ ( prim -- )
    S" VOID" TYPE NEWLINE
   