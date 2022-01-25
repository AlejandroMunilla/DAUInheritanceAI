# DAUInheritanceAI
Exemple of using Inheritance in Unity. Master class uses public virtual functions, while derived class uses public override functions. It is possible to extend master class funcitons with added behaviour to the existing master class by using base.NameOfTheFunciton on the derived class inside the function overriding the entire function. That way, you get all the commands / algorythms of the master class + those you want to add on the derived class. 

EnemyAI is the master class, while AIPolidoriArena is the class inheriting from EnemyAI. 
Note that using base.NameOfFunction allows to call the entire function from the master class inside the public override same function on the inherited class. 
