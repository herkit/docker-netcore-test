using Docker.DotNet;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DockerTest
{
    public class Worker : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var run = true;
            while (run)
            {
                Console.WriteLine("Enter something for the cow to say:");
                var text = Console.ReadLine();
                DockerClient client = new DockerClientConfiguration(
                    new Uri("http://localhost:2375"))
                     .CreateClient();

                var container = await client.Containers.CreateContainerAsync(new Docker.DotNet.Models.CreateContainerParameters
                {
                    Image = "cowsaid"
                });

                Console.WriteLine(container.ID);
                await client.Containers.StartContainerAsync(container.ID, new Docker.DotNet.Models.ContainerStartParameters {  });

                var inspect = await client.Containers.InspectContainerAsync(container.ID);
                Console.Write("Container name: " + inspect.Name);

                await client.Containers.WaitContainerAsync(container.ID);

                var archiveResult = await client.Containers.GetArchiveFromContainerAsync(container.ID, new Docker.DotNet.Models.GetArchiveFromContainerParameters { Path = "cowsaid.txt" }, false);


                using (var tarStream = new TarInputStream(archiveResult.Stream)) {
                    for (var entry = tarStream.GetNextEntry(); entry != null; entry = tarStream.GetNextEntry())
                    {
                        if (entry.Name == "cowsaid.txt") {
                            var tempFile = Path.GetTempFileName();
                            using (var file = File.OpenWrite(tempFile))
                            {
                                tarStream.CopyEntryContents(file);
                            }
                            Console.WriteLine("Extracted contents to " + tempFile);
                        }
                    }
                }

                Console.WriteLine("Removing container " + inspect.Name);
                await client.Containers.RemoveContainerAsync(container.ID, new Docker.DotNet.Models.ContainerRemoveParameters { RemoveVolumes = true });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
