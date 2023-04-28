# STILL WIP. DevShell 100% works, but i'm not sure in the package. It builds but i want to make sure it handles the icu-libs and i need to patch the csproj to false the invariantglobalization cuz whynot. 
{
  description = "A very basic .NET flake";

  inputs = { nixpkgs.url = "github:nixos/nixpkgs/nixos-unstable"; };
  outputs = { self, nixpkgs }:
    let
      system = "x86_64-linux";
      pkgs = nixpkgs.legacyPackages.${system};
    in {
      devShells.x86_64-linux.default = pkgs.mkShell {
        name = "i3helper";
        buildInputs = with pkgs; [ dotnet-sdk_7 clang zlib omnisharp-roslyn ];
				shellHook =
				''
					export DOTNET_ROOT=${pkgs.dotnet-sdk_7}
				'';
      };
      packages.${system}.default = pkgs.stdenv.mkDerivation {
        pname = "i3helper";
				version = "0.1";
        # unpackPhase = ":";
				src = pkgs.fetchFromGitHub {
					owner = "ShinyZero0";
					repo = "i3helper";
					rev = "master";
					sha256 = "sha256-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
				};
				nativeBuildInputs = with pkgs; [ zlib ];
				buildInputs = with pkgs; [ icu ];
        buildPhase = ''
					HOME=$PWD/home
					PATH=${pkgs.dotnet-sdk_7}/bin:$PATH
					DOTNET_ROOT=${pkgs.dotnet-sdk_7}

					mkdir -p $HOME
          dotnet publish -o ./out/
        '';

        installPhase = ''
          mkdir -p $out/bin
          cp ./out/descfmt $out/bin/
        '';
      };
    };
}
