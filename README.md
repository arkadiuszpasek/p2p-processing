# p2p-processing

### Structure
 - P2PProcessing is a dll library, can be built and installed into other projects
 - P2PProcessingConsole references this library directly (Visual Studio)

#### Classes
 - `P2P` - Library's facade, ready to be included in other applications
 - `Session` - main class representing session of a node, combines connections to other nodes and its logic
 - `Node Session` - represents Node's session to another node
 - `Connection` - wraps and represents connection to another node, adding a layer of abtraction between app logic and translation to data
 - `Msg` - represents objects containing informations sent to other nodes
 - `MsgBuff` - handles parsing of Msg to low level data type sent to other nodes and vice versa
### Running
 Run example app: `P2PProcessingConsole` - `Program.cs main()`