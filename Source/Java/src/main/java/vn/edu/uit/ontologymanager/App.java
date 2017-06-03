package vn.edu.uit;

import org.jetbrains.annotations.NotNull;
import vn.edu.uit.ontologymanager.OntologyManager;
import vn.edu.uit.server.Server;

import java.io.File;
import java.net.URL;
import java.nio.file.Paths;
import java.util.logging.Level;
import java.util.logging.Logger;

import static vn.edu.uit.server.Server.DEFAULT_PORT;

public class App {

    @NotNull
    private static final Logger LOGGER = Logger.getLogger(App.class.getName());

    public static void main(final String[] args) {
        try {
            final ClassLoader loader = ClassLoader.getSystemClassLoader();
            final URL resource = loader.getResource("rice.owl");
            final File file = Paths.get(resource.toURI()).toFile();

            final OntologyManager manager = new OntologyManager(file);

            final Server server = new Server(manager, DEFAULT_PORT);
            server.run();
        } catch (final Exception e) {
            LOGGER.log(Level.SEVERE, e.toString(), e);
        }
    }
}
