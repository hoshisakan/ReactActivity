FROM postgres:15.2

ARG POSTGRES_LANG_NAME
ARG POSTGRES_LANG_INPUTFILE
ARG POSTGRES_LANG_CHARMAP

RUN apt update && apt install -y iputils-ping telnet net-tools

# Set locale to specified language
RUN localedef -i ${POSTGRES_LANG_INPUTFILE} -c -f ${POSTGRES_LANG_CHARMAP} -A /usr/share/locale/locale.alias ${POSTGRES_LANG_NAME}