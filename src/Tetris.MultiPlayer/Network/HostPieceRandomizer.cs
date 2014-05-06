using System.Collections.Generic;
using System.Linq;

namespace Tetris.MultiPlayer.Network
{
    class HostPieceRandomizer
    {
        public readonly IDictionary<byte, IPieceGenerator> RealClientGenerators;
        public readonly IDictionary<byte, IPieceGenerator> RemoteClientGenerators;
        public readonly IPieceGenerator HostGenerator;

        public HostPieceRandomizer(byte[] clientIds)
        {
            var randomizer = new PieceRandomizer(clientIds.Length * 2 + 1);

            HostGenerator = randomizer.GetGenerator(0);

            RealClientGenerators = Enumerable.Range(0, clientIds.Length).ToDictionary(
                i => clientIds[i],
                i => randomizer.GetGenerator(1 + i));

            RemoteClientGenerators = Enumerable.Range(0, clientIds.Length).ToDictionary(
                i => clientIds[i],
                i => randomizer.GetGenerator(1 + clientIds.Length + i));
        }
    }
}
