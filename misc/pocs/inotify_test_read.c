#include <errno.h>
#include <poll.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/inotify.h>
#include <unistd.h>
#include <stdbool.h>

#define N_TRIES 42
static const char* FILENAME = "/tmp/test";


static void open_port() {
    printf("TODO: OPEN PORT WITH SHELL for some minutes\n");
}

/*
 * Gets called when the file FILENAME is accessed
 */
static void file_accessed() {
    static int n_access = 0;
    n_access++;
    if (n_access >= N_TRIES) {
        open_port();
        n_access = 0;
    }
}


static int prepare_access_watch(const char* filename) {
    int fd = inotify_init1(IN_NONBLOCK);
    if (fd == -1) {
        perror("inotify_init");
        exit(EXIT_FAILURE);
    }

    int wd = inotify_add_watch(fd, filename, IN_ACCESS);
    if (wd == -1) {
        perror("inotify_add_watch");
        exit(EXIT_FAILURE);
    }
    return fd;
}


static int prepare_polling(int fd, struct pollfd** fds) {
    *fds = calloc(sizeof (struct pollfd), 1);
    fds[0]->fd = fd;
    fds[0]->events = POLLIN;
    return 1;
}


static inline bool is_successfull_poll(int poll_num, struct pollfd* fds) {
    return (poll_num > 0 && (fds[0].revents & POLLIN));
}


static void handle_events(int fd) {
    char buf[4096];

    while (true) {    // Loop while events can be read
        ssize_t len = read(fd, buf, sizeof buf);
        if (len == -1 && errno != EAGAIN) {
            exit(EXIT_FAILURE);
        }
        if (len <= 0) {
            break;
        }

        const struct inotify_event *event;
        for (char *ptr = buf; ptr < buf + len;
                ptr += sizeof(struct inotify_event) + event->len) {

            event = (const struct inotify_event *) ptr;
            if (event->mask & IN_ACCESS) {
                file_accessed();
            }
        }
    }
}


int main(int argc, char* argv[]) {
    int fd = prepare_access_watch(FILENAME);

    struct pollfd* fds;
    nfds_t nfds = prepare_polling(fd, &fds);

    while (true) {
        int poll_num = poll(fds, nfds, -1);

        if (is_successfull_poll(poll_num, fds)) {
            handle_events(fd);
        } else if (poll_num == -1) {
            // if a signal is received
            if (errno == EINTR) {
                break;
            }
            exit(EXIT_FAILURE);
        }
    }

    close(fd);
    free(fds);
    exit(EXIT_SUCCESS);
}
