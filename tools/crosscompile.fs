\ $Id$

\ Cross-compile an interpreter for the host system, using the bootstrapped interpreter
\ generated by $(TOOLS)/primgen etc

include prelude.fs
include x-host.fs
save-image-to attila.c
bye
