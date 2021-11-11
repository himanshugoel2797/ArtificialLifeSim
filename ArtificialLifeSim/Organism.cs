using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ArtificialLifeSim.Physics;

namespace ArtificialLifeSim
{
    class OrganismLimits
    {
        public const float EyeDistanceEnergyFactor = 0.0001f;
        public const float BaseEyeEnergy = 0.001f;
        public const float BaseMuscleEnergy = 0.0001f;
        public const float EmptyUpkeepCost = 0.0001f;
        public const float BaseUpkeepCost = 0.001f;

        public const int MaxEyeCount = 0;
        public const int MaxMuscleCount = 4;
        public const int MaxMouthCount = 4;
        public const int MaxRotatorCount = 0;
        public const int MaxVertexCount = MaxEyeCount + MaxMuscleCount + MaxMouthCount + MaxRotatorCount;
        public const int MaxBrainInputCount = MaxEyeCount + MaxMouthCount; //Tick, Age, Energy, Health

        public const float FrictionFactor = 0.01f;
        public const float RotationFrictionFactor = 0.0f;
    }

    class ParameterGene : Gene
    {
        public float NecessaryEnergyLevelForReproduction { get; set; }
        public float NecessaryEnergyDurationForReproduction { get; set; }
        public float EnergyConsumptionForReproduction { get; set; }
        public float GrowthRate { get; set; }
        public float AsexualReproductionChance { get; set; }

        private float _visionRange;
        public float VisionRange { get { return _visionRange; } set { _visionRange = value; VisionRangeSquared = _visionRange * _visionRange; } }
        public float VisionRangeSquared { get; private set; }

        public void Generate()
        {
            NecessaryEnergyLevelForReproduction = (float)Utils.RandomDouble(0.1, 0.5);
            NecessaryEnergyDurationForReproduction = (float)Utils.RandomDouble(0.1, 10.0);
            EnergyConsumptionForReproduction = (float)Utils.RandomDouble(0.1, 2);
            AsexualReproductionChance = (float)Utils.RandomDouble(0.01, 0.05);
            GrowthRate = (float)Utils.RandomDouble(0.1, 0.5);
            VisionRange = (float)Utils.RandomDouble(0.1, 50);
        }

        public bool IsViable()
        {
            return true;
        }

        public Gene Mate(Genome genome0, Genome genome1, Gene other, MutationOptions options)
        {
            var o = other as ParameterGene;
            var gene = new ParameterGene();
            gene.NecessaryEnergyLevelForReproduction = (NecessaryEnergyLevelForReproduction + o.NecessaryEnergyLevelForReproduction) / 2;
            gene.NecessaryEnergyDurationForReproduction = (NecessaryEnergyDurationForReproduction + o.NecessaryEnergyDurationForReproduction) / 2;
            gene.EnergyConsumptionForReproduction = (EnergyConsumptionForReproduction + o.EnergyConsumptionForReproduction) / 2;
            gene.AsexualReproductionChance = (AsexualReproductionChance + o.AsexualReproductionChance) / 2;
            gene.GrowthRate = (GrowthRate + o.GrowthRate) / 2;
            gene.VisionRange = (VisionRange + o.VisionRange) / 2;
            return gene;
        }

        public Gene Mutate(Genome genome, MutationOptions options)
        {
            var gene = new ParameterGene();
            gene.NecessaryEnergyLevelForReproduction = Math.Clamp(NecessaryEnergyLevelForReproduction + (float)Utils.RandomDouble(-0.1, 0.1), 0.0f, 1.0f);
            gene.NecessaryEnergyDurationForReproduction = Math.Clamp(NecessaryEnergyDurationForReproduction + (float)Utils.RandomDouble(-0.1, 0.1), 1.0f, 10.0f);
            gene.EnergyConsumptionForReproduction = Math.Clamp(EnergyConsumptionForReproduction + (float)Utils.RandomDouble(-0.1, 0.1), 0.2f, 2.0f);
            gene.AsexualReproductionChance = Math.Clamp(AsexualReproductionChance + (float)Utils.RandomDouble(-0.01, 0.01), 0, 0.01f);
            gene.GrowthRate = Math.Clamp(GrowthRate + (float)Utils.RandomDouble(-0.1, 0.1), 0.01f, 0.1f);
            gene.VisionRange = Math.Clamp(VisionRange + (float)Utils.RandomDouble(-4, 4), 0.01f, 50f);
            return gene;
        }

        internal void Update(BodyGene body)
        {
            //EnergyConsumptionForReproduction = (float)Utils.RandomDouble(0.1, 0.9) + body.CalculateArea() * 0.5f;
        }
    }

    class Organism : IPosition
    {
        public World World { get; set; }
        public Genome Genome { get; private set; }
        public float Age { get; private set; }
        public float Energy { get; set; }    // Energy is the amount of energy the organism has, gradually reduces as the organism does anything.
        public float Health { get; set; }   //Increases to 1 while energy is greater than 0, organism dies when this reaches 0.
        public float LastReproducedAge { get; private set; }
        public BrainGene Brain { get; private set; }
        public BodyGene Body { get; private set; }
        public ParameterGene Parameters { get; private set; }
        public Hull Hull { get; private set; }
        public Vector2 Position { get { return Hull.Position; } set { Hull.Position = value; } }
        public float Radius { get { return Hull.Radius; } set { Hull.Radius = value; } }

        private float[] muscleStates;
        private float[] rotatorStates;

        public static float HighestAge;
        public static Vector2 HighestPosition;
        public static float HighestEnergy;
        public static float HighestHealth;
        public static ParameterGene HighestParameters;
        static object HighestLock = new object();

        public Organism(World world, Vector2 position)
        {
            Brain = new BrainGene();
            Body = new BodyGene();
            Parameters = new ParameterGene();
            Genome = new Genome(Brain, Body, Parameters);
            World = world;
            Hull = new Hull(Body.Vertices.ToArray(), position, Body.CalculateArea(), world.Side);
            Age = (float)Utils.RandomDouble(0, 100);
            Energy = 1;
            Health = 1;
        }

        public void Generate()
        {
            Brain.Generate();
            Body.Generate();
            Parameters.Generate();
            Parameters.Update(Body);

            RebuildHull();
        }

        public bool IsAlive()
        {
            return Health > 0 && Energy > 0 && Brain.IsViable() && Body.IsViable() && Parameters.IsViable();
        }

        public void Mutate(MutationOptions options)
        {
            Brain = (BrainGene)Brain.Mutate(Genome, options);
            Body = (BodyGene)Body.Mutate(Genome, options);
            Parameters = (ParameterGene)Parameters.Mutate(Genome, options);

            RebuildHull();
        }

        public void RebuildHull()
        {
            Hull = new Hull(Body.Vertices.ToArray(), Position, Body.CalculateArea(), World.Side);
            Hull.Velocity = Utils.RandomVector2(-0.1f, 0.1f);
        }

        public void Reproduce(Organism mate, MutationOptions options)
        {
            if (World.Organisms.Count >= World.MaxPopulation)
                return;

            if (mate == null)
            {
                //if (Energy < Parameters.NecessaryEnergyLevelForReproduction)
                //    return;
                if (Age - LastReproducedAge < Parameters.NecessaryEnergyDurationForReproduction)
                    return;
                if (Energy < Parameters.EnergyConsumptionForReproduction)
                    return;

                //TODO: Make sure there is space for the offspring.

                if (Utils.RandomDouble() < Parameters.AsexualReproductionChance)
                {
                    Organism child = new Organism(World, Position + Utils.RandomVector2(Hull.Radius, Hull.Radius * 4));
                    child.Energy = Parameters.EnergyConsumptionForReproduction;
                    child.Brain = (BrainGene)Brain.Mutate(Genome, options);
                    child.Body = (BodyGene)Body.Mutate(Genome, options);
                    child.Parameters = (ParameterGene)Parameters.Mutate(Genome, options);
                    child.RebuildHull();

                    if (child.IsAlive())
                    {
                        Energy -= Parameters.EnergyConsumptionForReproduction;
                        child.Hull.Velocity = Vector2.Normalize(Utils.RandomVector2(0, 1)) * Hull.Velocity.Length();
                        World.AddOrganism(child);
                        LastReproducedAge = Age;
                    }
                }
            }
            else
            {
                //if (Energy < Parameters.NecessaryEnergyLevelForReproduction)
                //    return;
                //if (mate.Energy < Parameters.NecessaryEnergyLevelForReproduction)
                //    return;
                if (Age - LastReproducedAge < Parameters.NecessaryEnergyDurationForReproduction)
                    return;
                if (mate.Age - mate.LastReproducedAge < mate.Parameters.NecessaryEnergyDurationForReproduction)
                    return;
                if (Energy < Parameters.EnergyConsumptionForReproduction * 0.5)
                    return;
                if (mate.Energy < mate.Parameters.EnergyConsumptionForReproduction * 0.5)
                    return;
                if ((mate.Position - Position).LengthSquared() > 4)
                    return;
                //TODO: Make sure there is space for the offspring.


                Organism child = new Organism(World, Position + Utils.RandomVector2(Hull.Radius, Hull.Radius * 4));
                child.Energy = (0.5f * Parameters.EnergyConsumptionForReproduction + 0.5f * mate.Parameters.EnergyConsumptionForReproduction);
                child.Brain = (BrainGene)Brain.Mate(Genome, mate.Genome, mate.Brain, options);
                child.Body = (BodyGene)Body.Mate(Genome, mate.Genome, mate.Body, options);
                child.Parameters = (ParameterGene)Parameters.Mate(Genome, mate.Genome, mate.Parameters, options);

                child.Mutate(options);

                if (child.IsAlive())
                {
                    mate.Energy -= Parameters.EnergyConsumptionForReproduction * 0.5f;
                    Energy -= Parameters.EnergyConsumptionForReproduction * 0.5f;
                    child.Hull.Velocity = Vector2.Normalize(Utils.RandomVector2(0, 1)) * Hull.Velocity.Length();
                    World.AddOrganism(child);
                    LastReproducedAge = Age;
                    mate.LastReproducedAge = mate.Age;
                }
            }
        }

        public void Update(float tickVal)
        {
            Age += 0.1f; //Parameters.GrowthRate;
            Body.Rotate(Hull.DeltaRotation);

            //Update brain
            float[] eyeSensors = new float[OrganismLimits.MaxEyeCount];
            float[] mouthSensors = new float[OrganismLimits.MaxMouthCount];
            if (muscleStates == null)
            {
                muscleStates = new float[OrganismLimits.MaxMuscleCount];
                rotatorStates = new float[OrganismLimits.MaxRotatorCount];
            }

            //Update eye and mouth sensors
            var eyesAvailable = Body.Vertices.Zip(Body.VertexTypes).Where(x => x.Item2 == BodyVertexType.Eye).Select(x => x.Item1).ToArray();
            var mouthsAvailable = Body.Vertices.Zip(Body.VertexTypes).Where(x => x.Item2 == BodyVertexType.Mouth).Select(x => x.Item1).ToArray();

            for (int i = 0; i < eyesAvailable.Length; i++)
            {
                var visibleToEye = World.GetOrganismsInContext(this, this.Position + eyesAvailable[i]);
                if (visibleToEye.Count() > 0)
                    eyeSensors[i] = (visibleToEye[0].Position - (eyesAvailable[i] + this.Position)).LengthSquared() / Parameters.VisionRangeSquared;
            }

            for (int i = 0; i < mouthsAvailable.Length; i++)
            {
                float curMouthValue = 1.0f;
                var foodVisibleToMouth = World.GetFoodInContext(this, this.Position + mouthsAvailable[i]);
                if (foodVisibleToMouth.Count() > 0)
                    curMouthValue = (foodVisibleToMouth[0].Position - (mouthsAvailable[i] + this.Position)).LengthSquared() / Parameters.VisionRangeSquared;
                mouthSensors[i] = curMouthValue;
            }

            float[] inputs = new float[OrganismLimits.MaxBrainInputCount];
            //Array.Copy(muscleStates, 0, inputs, 0, muscleStates.Length);
            //Array.Copy(rotatorStates, 0, inputs, muscleStates.Length, rotatorStates.Length);
            Array.Copy(eyeSensors, 0, inputs, 0, eyeSensors.Length);
            Array.Copy(mouthSensors, 0, inputs, eyeSensors.Length, mouthSensors.Length);
            //inputs[inputs.Length - 1] = tickVal;
            //inputs[inputs.Length - 2] = Age;
            //inputs[inputs.Length - 3] = Energy;
            //inputs[inputs.Length - 4] = Health;

            float[] outputs = Brain.Evaluate(inputs);
            Array.Copy(outputs, 0, muscleStates, 0, muscleStates.Length);
            Array.Copy(outputs, muscleStates.Length, rotatorStates, 0, rotatorStates.Length);

            //Process energy consumption
            Energy -= OrganismLimits.BaseUpkeepCost * Body.CalculateArea();
            Energy -= OrganismLimits.EmptyUpkeepCost * Body.VertexTypes.Count(x => x == BodyVertexType.None);
            Energy -= OrganismLimits.BaseEyeEnergy * Body.VertexTypes.Count(x => x == BodyVertexType.Eye) + OrganismLimits.EyeDistanceEnergyFactor * Parameters.VisionRange;

            //Update body
            //Apply rotators
            int rotatorCount = Body.VertexTypes.Count(x => x == BodyVertexType.Rotator);
            for (int i = 0; i < rotatorStates.Length; i++)
                if (i < rotatorCount)
                {
                    if (Energy > OrganismLimits.BaseMuscleEnergy * Math.Abs(rotatorStates[i]))
                    {
                        Energy -= OrganismLimits.BaseMuscleEnergy * Math.Abs(rotatorStates[i]);
                        Hull.ApplyRotation(rotatorStates[i]);
                    }
                }

            //Apply muscles
            int muscleCount = Body.VertexTypes.Count(x => x == BodyVertexType.Muscle);
            var muscles = Body.Vertices.Zip(Body.VertexTypes).Where(x => x.Item2 == BodyVertexType.Muscle).Select(x => x.Item1).ToArray();
            for (int i = 0; i < muscleStates.Length; i++)
                if (i < muscleCount)
                {
                    if (Energy > OrganismLimits.BaseMuscleEnergy * Math.Abs(muscleStates[i]))
                    {
                        Energy -= OrganismLimits.BaseMuscleEnergy * Math.Abs(muscleStates[i]);
                        Hull.ApplyPush(muscleStates[i], -muscles[i]);
                    }
                }

            //Consume food
            var foodVisible = World.GetFoodInContext(this, this.Position, false);
            if (foodVisible.Count() > 0 && ((foodVisible[0].Position - Position).LengthSquared() <= MathF.Pow(foodVisible[0].Radius + Hull.Radius, 2)) && Energy < 10)
                lock (foodVisible[0])
                {
                    Energy += foodVisible[0].Energy;
                    foodVisible[0].Energy = 0;
                }

            //Process health
            if (Energy < 0)
                Energy = 0;

            if (Energy <= 0)
                Health = 0.0f;
            //else if (Health < 1.0f)
            //    Health += 0.01f;

            //if (Health > 1.0f)
            //    Health = 1.0f;

            //if (Health < 0)
            //    Health = 0;


            //Check if organism can reproduce and if so, do so.
            var potentialMate = World.GetOrganismsInContext(this, Position);
            if (potentialMate.Length > 0)
                Reproduce(potentialMate[0], World.MutationOptions);
            else
                Reproduce(null, World.MutationOptions);
            //TODO: Consume other organism if not reproducing

            lock (HighestLock)
            {
                if (HighestAge < Age)
                {
                    HighestAge = Age;
                    HighestEnergy = Energy;
                    HighestHealth = Health;
                    HighestPosition = Position;
                    HighestParameters = Parameters;
                }
            }
        }

    }
}
