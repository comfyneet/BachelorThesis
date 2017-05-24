package vn.edu.uit.server;

import org.jetbrains.annotations.NotNull;
import vn.edu.uit.ontologymanager.OntologyManager;

import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;
import java.net.ServerSocket;
import java.net.Socket;
import java.nio.ByteBuffer;
import java.nio.charset.Charset;
import java.util.logging.Logger;

public class Server {
    public static final int DEFAULT_PORT = 2002;

    @NotNull
    private static final Logger LOGGER = Logger.getLogger(Server.class.getName());
    @NotNull
    private final OntologyManager manager;
    private final int port;
    private boolean running;

    public Server(@NotNull final OntologyManager manager, final int port) {
        this.manager = manager;
        this.port = port;
    }

    public final void stop() {
        running = false;
    }

    public final void run() throws IOException {
        final ServerSocket serverSocket = new ServerSocket(port);

        running = true;
        while (running) {
            final Socket socket = serverSocket.accept();

            final InputStream inputStream = socket.getInputStream();
            final OutputStream outputStream = socket.getOutputStream();

            final byte[] receivedLengthInBytes = new byte[4];
            inputStream.read(receivedLengthInBytes, 0, 4);
            final int receivedLength = (((receivedLengthInBytes[3] & 0xff) << 24) | ((receivedLengthInBytes[2] & 0xff) << 16) | ((receivedLengthInBytes[1] & 0xff) << 8) | (receivedLengthInBytes[0] & 0xff));
            final byte[] receivedBytes = new byte[receivedLength];
            inputStream.read(receivedBytes, 0, receivedLength);
            final String receivedMessage = new String(receivedBytes, "UTF-8");
            LOGGER.info("Server receives " + receivedLength + " bytes: \"" + receivedMessage + "\"");

            final String resultMessage = manager.process(receivedMessage);

            final ByteBuffer sendingByteBuffer = Charset.forName("UTF-8").encode(resultMessage);
            final byte[] sendingBytes = new byte[sendingByteBuffer.remaining()];
            sendingByteBuffer.get(sendingBytes);
            final int sendingLength = sendingBytes.length;
            final byte[] sendingLengthInBytes = new byte[4];
            sendingLengthInBytes[0] = (byte) (sendingLength & 0xff);
            sendingLengthInBytes[1] = (byte) ((sendingLength >> 8) & 0xff);
            sendingLengthInBytes[2] = (byte) ((sendingLength >> 16) & 0xff);
            sendingLengthInBytes[3] = (byte) ((sendingLength >> 24) & 0xff);
            outputStream.write(sendingLengthInBytes);
            outputStream.write(sendingBytes);
            LOGGER.info("Server sends " + sendingLength + " bytes: \"" + resultMessage + "\"");

            socket.close();
        }

        serverSocket.close();
    }
}
